using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace CustomControlResources
{
    public class GifImage : UserControl
    {
        private GifAnimation _gifAnimation;
        private Image _image;

        #region DependencyProperty ForceGifAnim
        public static readonly DependencyProperty ForceGifAnimProperty =
            DependencyProperty.Register("ForceGifAnim", typeof(bool), typeof(GifImage), new FrameworkPropertyMetadata(false));
        public bool ForceGifAnim
        {
            get { return (bool)GetValue(ForceGifAnimProperty); }
            set { SetValue(ForceGifAnimProperty, value); }
        }
        #endregion

        #region DependencyProperty Source
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(GifImage),
                                        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage)d;
            var s = (string)e.NewValue;
            obj.CreateFromSourceString(s);
        }
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        #endregion

        #region DependencyProperty Stretch
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(GifImage), new FrameworkPropertyMetadata(Stretch.Fill, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStretchChanged));
        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage)d;
            var s = (Stretch)e.NewValue;
            if (obj._gifAnimation != null)
                obj._gifAnimation.Stretch = s;
            else if (obj._image != null)
                obj._image.Stretch = s;
        }
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }
        #endregion

        #region DependencyProperty StretchDirection
        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(GifImage), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStretchDirectionChanged));
        private static void OnStretchDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage)d;
            var s = (StretchDirection)e.NewValue;
            if (obj._gifAnimation != null)
                obj._gifAnimation.StretchDirection = s;
            else if (obj._image != null)
                obj._image.StretchDirection = s;
        }
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }
        #endregion

        public delegate void ExceptionRoutedEventHandler(object sender, GifImageExceptionRoutedEventArgs args);

        public static readonly RoutedEvent ImageFailedEvent = EventManager.RegisterRoutedEvent("ImageFailed", RoutingStrategy.Bubble, typeof(ExceptionRoutedEventHandler), typeof(GifImage));

        public event ExceptionRoutedEventHandler ImageFailed
        {
            add { AddHandler(ImageFailedEvent, value); }
            remove { RemoveHandler(ImageFailedEvent, value); }
        }

        void ImageImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            RaiseImageFailedEvent(e.ErrorException);
        }

        void RaiseImageFailedEvent(Exception exp)
        {
            var newArgs = new GifImageExceptionRoutedEventArgs(ImageFailedEvent, this) { ErrorException = exp };
            RaiseEvent(newArgs);
        }

        private void DeletePreviousImage()
        {
            if (_image != null)
            {
                RemoveLogicalChild(_image);
                _image = null;
            }
            if (_gifAnimation != null)
            {
                RemoveLogicalChild(_gifAnimation);
                _gifAnimation = null;
            }
        }

        private void CreateNonGifAnimationImage()
        {
            _image = new Image();
            _image.ImageFailed += ImageImageFailed;
            var src = (ImageSource)(new ImageSourceConverter().ConvertFromString(Source));
            _image.Source = src;
            _image.Stretch = Stretch;
            _image.StretchDirection = StretchDirection;
            AddChild(_image);
        }

        private void CreateGifAnimation(MemoryStream memoryStream)
        {
            _gifAnimation = new GifAnimation();
            _gifAnimation.CreateGifAnimation(memoryStream);
            _gifAnimation.Stretch = Stretch;
            _gifAnimation.StretchDirection = StretchDirection;
            AddChild(_gifAnimation);
        }

        private void CreateFromSourceString(string source)
        {
            DeletePreviousImage();
            Uri uri;

            try
            {
                uri = new Uri(source, UriKind.RelativeOrAbsolute);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
                return;
            }

            if (source.Trim().ToUpper().EndsWith(".GIF") || ForceGifAnim)
            {
                if (!uri.IsAbsoluteUri)
                    GetGifStreamFromPack(uri);
                else
                {
                    var leftPart = uri.GetLeftPart(UriPartial.Scheme);
                    switch (leftPart)
                    {
                        case "file://":
                        case "ftp://":
                        case "http://":
                            GetGifStreamFromHttp(uri);
                            break;
                        case "pack://":
                            GetGifStreamFromPack(uri);
                            break;
                        default:
                            CreateNonGifAnimationImage();
                            break;
                    }
                }
            }
            else
                CreateNonGifAnimationImage();
        }

        private delegate void WebRequestFinishedDelegate(MemoryStream memoryStream);

        private void WebRequestFinished(MemoryStream memoryStream)
        {
            CreateGifAnimation(memoryStream);
        }

        private delegate void WebRequestErrorDelegate(Exception exp);

        private void WebRequestError(Exception exp)
        {
            RaiseImageFailedEvent(exp);
        }

        private void WebResponseCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState)asyncResult.AsyncState;
            try
            {
                var webResponse = webReadState.WebRequest.EndGetResponse(asyncResult);
                webReadState.ReadStream = webResponse.GetResponseStream();
                webReadState.Buffer = new byte[100000];
                if (webReadState.ReadStream != null)
                    webReadState.ReadStream.BeginRead(webReadState.Buffer, 0, webReadState.Buffer.Length, WebReadCallback, webReadState);
            }
            catch (WebException exp)
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
            }
        }

        private void WebReadCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState)asyncResult.AsyncState;
            var count = webReadState.ReadStream.EndRead(asyncResult);
            if (count > 0)
            {
                webReadState.MemoryStream.Write(webReadState.Buffer, 0, count);
                try
                {
                    webReadState.ReadStream.BeginRead(webReadState.Buffer, 0, webReadState.Buffer.Length, WebReadCallback, webReadState);
                }
                catch (WebException exp)
                {
                    Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
                }
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestFinishedDelegate(WebRequestFinished), webReadState.MemoryStream);
            }
        }

        private void GetGifStreamFromHttp(Uri uri)
        {
            try
            {
                var webReadState = new WebReadState
                {
                    MemoryStream = new MemoryStream(),
                    WebRequest = WebRequest.Create(uri)
                };
                webReadState.WebRequest.Timeout = 10000;

                webReadState.WebRequest.BeginGetResponse(WebResponseCallback, webReadState);
            }
            catch (SecurityException)
            {
                CreateNonGifAnimationImage();
            }
        }

        private void ReadGifStreamSynch(Stream s)
        {
            MemoryStream memoryStream;
            using (s)
            {
                memoryStream = new MemoryStream((int)s.Length);
                var br = new BinaryReader(s);
                var gifData = br.ReadBytes((int)s.Length);
                memoryStream.Write(gifData, 0, (int)s.Length);
                memoryStream.Flush();
            }
            CreateGifAnimation(memoryStream);
        }

        private void GetGifStreamFromPack(Uri uri)
        {
            try
            {
                StreamResourceInfo streamInfo;

                if (!uri.IsAbsoluteUri)
                {
                    streamInfo = Application.GetContentStream(uri) ?? Application.GetResourceStream(uri);
                }
                else
                {
                    if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin"))
                    {
                        streamInfo = Application.GetRemoteStream(uri);
                    }
                    else
                    {
                        streamInfo = Application.GetContentStream(uri) ?? Application.GetResourceStream(uri);
                    }
                }
                if (streamInfo == null)
                {
                    throw new FileNotFoundException("Resource not found.", uri.ToString());
                }
                ReadGifStreamSynch(streamInfo.Stream);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
            }
        }
    }

    class GifAnimation : Viewbox
    {

        private class GifFrame : Image
        {

            public int DelayTime;

            public int DisposalMethod;

            public int Left;

            public int Top;

            public int FrameWidth;

            public int FrameHeight;
        }

        private readonly Canvas _canvas;

        private List<GifFrame> _frameList;

        private int _frameCounter;
        private int _numberOfFrames;

        private int _numberOfLoops = -1;
        private int _currentLoop;

        private int _logicalWidth;
        private int _logicalHeight;

        private DispatcherTimer _frameTimer;

        private GifFrame _currentParseGifFrame;

        public GifAnimation()
        {
            _canvas = new Canvas();
            this.Child = _canvas;
        }

        private void Reset()
        {
            if (_frameList != null)
            {
                _frameList.Clear();
            }
            _frameList = null;
            _frameCounter = 0;
            _numberOfFrames = 0;
            _numberOfLoops = -1;
            _currentLoop = 0;
            _logicalWidth = 0;
            _logicalHeight = 0;
            if (_frameTimer == null) return;
            _frameTimer.Stop();
            _frameTimer = null;
        }

        #region PARSE
        private void ParseGif(byte[] gifData)
        {
            _frameList = new List<GifFrame>();
            _currentParseGifFrame = new GifFrame();
            ParseGifDataStream(gifData, 0);
        }


        private int ParseBlock(byte[] gifData, int offset)
        {
            switch (gifData[offset])
            {
                case 0x21:
                    return gifData[offset + 1] == 0xF9
                               ? ParseGraphicControlExtension(gifData, offset)
                               : ParseExtensionBlock(gifData, offset);
                case 0x2C:
                    offset = ParseGraphicBlock(gifData, offset);
                    _frameList.Add(_currentParseGifFrame);
                    _currentParseGifFrame = new GifFrame();
                    return offset;
                case 0x3B:
                    return -1;
                default:
                    throw new Exception("GIF format incorrect: missing graphic block or special-purpose block. ");
            }
        }

        private int ParseGraphicControlExtension(byte[] gifData, int offset)
        {
            int length = gifData[offset + 2];
            var returnOffset = offset + length + 2 + 1;

            var packedField = gifData[offset + 3];
            _currentParseGifFrame.DisposalMethod = (packedField & 0x1C) >> 2;

            int delay = BitConverter.ToUInt16(gifData, offset + 4);
            _currentParseGifFrame.DelayTime = delay;
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        private int ParseLogicalScreen(byte[] gifData, int offset)
        {
            _logicalWidth = BitConverter.ToUInt16(gifData, offset);
            _logicalHeight = BitConverter.ToUInt16(gifData, offset + 2);

            var packedField = gifData[offset + 4];
            var hasGlobalColorTable = (packedField & 0x80) > 0;

            var currentIndex = offset + 7;
            if (hasGlobalColorTable)
            {
                var colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            return currentIndex;
        }

        private int ParseGraphicBlock(byte[] gifData, int offset)
        {
            _currentParseGifFrame.Left = BitConverter.ToUInt16(gifData, offset + 1);
            _currentParseGifFrame.Top = BitConverter.ToUInt16(gifData, offset + 3);
            _currentParseGifFrame.FrameWidth = BitConverter.ToUInt16(gifData, offset + 5);
            _currentParseGifFrame.FrameHeight = BitConverter.ToUInt16(gifData, offset + 7);
            if (_currentParseGifFrame.FrameWidth > _logicalWidth)
            {
                _logicalWidth = _currentParseGifFrame.FrameWidth;
            }
            if (_currentParseGifFrame.FrameHeight > _logicalHeight)
            {
                _logicalHeight = _currentParseGifFrame.FrameHeight;
            }
            var packedField = gifData[offset + 9];
            var hasLocalColorTable = (packedField & 0x80) > 0;

            var currentIndex = offset + 9;
            if (hasLocalColorTable)
            {
                var colorTableLength = packedField & 0x07;
                colorTableLength = (int)Math.Pow(2, colorTableLength + 1) * 3;
                currentIndex = currentIndex + colorTableLength;
            }
            currentIndex++;

            currentIndex++;

            while (gifData[currentIndex] != 0x00)
            {
                currentIndex = currentIndex + gifData[currentIndex];
                currentIndex++;
            }
            currentIndex = currentIndex + 1;
            return currentIndex;
        }

        private int ParseExtensionBlock(byte[] gifData, int offset)
        {
            int length = gifData[offset + 2];
            var returnOffset = offset + length + 2 + 1;
            if (gifData[offset + 1] == 0xFF && length > 10)
            {
                var netscape = System.Text.Encoding.ASCII.GetString(gifData, offset + 3, 8);
                if (netscape == "NETSCAPE")
                {
                    _numberOfLoops = BitConverter.ToUInt16(gifData, offset + 16);
                    if (_numberOfLoops > 0)
                    {
                        _numberOfLoops++;
                    }
                }
            }
            while (gifData[returnOffset] != 0x00)
            {
                returnOffset = returnOffset + gifData[returnOffset] + 1;
            }

            returnOffset++;

            return returnOffset;
        }

        private int ParseHeader(byte[] gifData, int offset)
        {
            var str = System.Text.Encoding.ASCII.GetString(gifData, offset, 3);
            if (str != "GIF")
            {
                throw new Exception("Not a proper GIF file: missing GIF header");
            }
            return 6;
        }

        private void ParseGifDataStream(byte[] gifData, int offset)
        {
            offset = ParseHeader(gifData, offset);
            offset = ParseLogicalScreen(gifData, offset);
            while (offset != -1)
            {
                offset = ParseBlock(gifData, offset);
            }
        }

        #endregion

        public void CreateGifAnimation(MemoryStream memoryStream)
        {
            Reset();

            var gifData = memoryStream.GetBuffer();  // Use GetBuffer so that there is no memory copy

            var decoder = new GifBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            _numberOfFrames = decoder.Frames.Count;

            try
            {
                ParseGif(gifData);
            }
            catch
            {
                throw new FileFormatException("Unable to parse Gif file format.");
            }

            for (int i = 0; i < decoder.Frames.Count; i++)
            {
                _frameList[i].Source = decoder.Frames[i];
                _frameList[i].Visibility = Visibility.Hidden;
                _canvas.Children.Add(_frameList[i]);
                Canvas.SetLeft(_frameList[i], _frameList[i].Left);
                Canvas.SetTop(_frameList[i], _frameList[i].Top);
                Panel.SetZIndex(_frameList[i], i);
            }
            _canvas.Height = _logicalHeight;
            _canvas.Width = _logicalWidth;

            _frameList[0].Visibility = Visibility.Visible;

            //foreach (var frame in _frameList)
            //{
            //    Console.WriteLine(frame.DisposalMethod.ToString() + " " + frame.FrameWidth.ToString() + " " + frame.DelayTime.ToString());
            //}

            if (_frameList.Count <= 1) return;
            if (_numberOfLoops == -1)
            {
                _numberOfLoops = 1;
            }
            _frameTimer = new DispatcherTimer();
            _frameTimer.Tick += NextFrame;
            _frameTimer.Interval = new TimeSpan(0, 0, 0, 0, _frameList[0].DelayTime * 10);
            _frameTimer.Start();
        }

        public void NextFrame()
        {
            NextFrame(null, null);
        }

        public void NextFrame(object sender, EventArgs e)
        {
            _frameTimer.Stop();
            if (_numberOfFrames == 0) return;
            if (_frameList[_frameCounter].DisposalMethod == 2)
            {
                _frameList[_frameCounter].Visibility = Visibility.Hidden;
            }
            if (_frameList[_frameCounter].DisposalMethod >= 3)
            {
                _frameList[_frameCounter].Visibility = Visibility.Hidden;
            }
            _frameCounter++;

            if (_frameCounter < _numberOfFrames)
            {
                _frameList[_frameCounter].Visibility = Visibility.Visible;
                _frameTimer.Interval = new TimeSpan(0, 0, 0, 0, _frameList[_frameCounter].DelayTime * 10);
                _frameTimer.Start();
            }
            else
            {
                if (_numberOfLoops != 0)
                {
                    _currentLoop++;
                }
                if (_currentLoop < _numberOfLoops || _numberOfLoops == 0)
                {
                    foreach (var frame in _frameList)
                    {
                        frame.Visibility = Visibility.Hidden;
                    }
                    _frameCounter = 0;
                    _frameList[_frameCounter].Visibility = Visibility.Visible;
                    _frameTimer.Interval = new TimeSpan(0, 0, 0, 0, _frameList[_frameCounter].DelayTime * 10);
                    _frameTimer.Start();
                }
            }
        }
    }

    public class GifImageExceptionRoutedEventArgs : RoutedEventArgs
    {
        public Exception ErrorException;

        public GifImageExceptionRoutedEventArgs(RoutedEvent routedEvent, object obj)
            : base(routedEvent, obj)
        {
        }
    }

    class WebReadState
    {
        public WebRequest WebRequest;
        public MemoryStream MemoryStream;
        public Stream ReadStream;
        public byte[] Buffer;
    }
}

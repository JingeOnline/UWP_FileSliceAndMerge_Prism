using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class TxtSliceInfoModel:BindableBase
    {
        public StorageFile StorageFile { get; set; }
        public string MergedFileName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public uint Index { get; set; }

        private string _textContent;
        public string TextContent
        {
            get { return _textContent; }
            set { SetProperty(ref _textContent, value); }
        }
        //private long _textLength;
        public int TextLength
        {
            get { return TextContent.Length; }
        }

        private long _txtWordCount;
        public long TxtWordCount
        {
            get { return _txtWordCount; }
            set { SetProperty(ref _txtWordCount, value); }
        }
        private long _txtLineCount;
        public long TxtLineCount
        {
            get { return _txtLineCount; }
            set { SetProperty(ref _txtLineCount, value); }
        }

        private bool _isStart;
        public bool IsStart
        {
            get { return _isStart; }
            set { SetProperty(ref _isStart, value); }
        }

        private bool _isDone;
        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                SetProperty(ref _isDone, value);
                if (IsDone)
                {
                    Icon = "\xF78C";
                    IsStart = false;
                }
            }
        }

        private string _icon = "\xF5ED";
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class TxtEntiretyInfoModel: BindableBase
    {
        public StorageFile StorageFile { get; set; }
        public String FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText
        {
            //每三位数添加一个逗号
            get { return FileSize.ToString("N0"); }
        }
        private bool isStart = false;
        public bool IsStart
        {
            get { return isStart; }
            set { SetProperty(ref isStart, value); }
        }
        private bool isDone = false;
        public bool IsDone
        {
            get { return isDone; }
            set
            {
                SetProperty(ref isDone, value);
                if (value)
                {
                    Icon = "\xF78C";
                    IsStart = false;
                }
            }
        }
        private string icon = "\xE160";
        public string Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }
        private int sliceNumber = 0;
        public int SliceNumber
        {
            get { return sliceNumber; }
            set { SetProperty(ref sliceNumber, value); }
        }
        private int sliceCompletedNumber = 0;
        public int SliceComplatedNumber
        {
            get { return sliceCompletedNumber; }
            set
            {
                SetProperty(ref sliceCompletedNumber, value);
                if (sliceCompletedNumber == SliceNumber)
                {
                    IsDone = true;
                }
            }
        }

        private long _txtWordCount;
        public long TxtWordCount
        {
            get { return _txtWordCount; }
            set { SetProperty(ref _txtWordCount,value); }
        }
        private long _txtLineCount;
        public long TxtLineCount
        {
            get { return _txtLineCount; }
            set { SetProperty(ref _txtLineCount, value); }
        }
        private int _textLength;
        public int TextLength
        {
            get { return _textLength; }
            set { SetProperty(ref _textLength, value); }
        }
        private string _textContent;
        public string TextContent
        {
            get { return _textContent; }
            set
            {
                SetProperty(ref _textContent, value);
                if (_textContent != null)
                {
                    TextLength = _textContent.Length;
                }
            }
        }
    }
}

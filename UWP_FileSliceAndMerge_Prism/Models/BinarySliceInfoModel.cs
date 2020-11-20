using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.Storage;
using Prism.Mvvm;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class BinarySliceInfoModel : BindableBase
    {
        public StorageFile StorageFile { get; set; }
        public string MergedFileName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public uint Index { get; set; }
        //public string MergedFileName { get; set; }
        public string FileSizeText
        {
            //每三位数添加一个逗号
            get { return FileSize.ToString("N0"); }
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
                SetProperty(ref _isDone,value);
                if (IsDone)
                {
                    Icon = "\xF78C";
                    IsStart = false;
                }
            }
        }

        private long _finishSize = 0;
        public long FinishSize
        {
            get { return _finishSize; }
            set
            {
                SetProperty(ref _finishSize, value);
            }
        }

        private string _icon = "\xF5ED";
        public string Icon
        {
            get { return _icon; }
            set
            {
                SetProperty(ref _icon, value);
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string propName = "")
        //{
        //    if (this.PropertyChanged != null)
        //    {
        //        var handler = PropertyChanged;
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //    }
        //}
    }
}

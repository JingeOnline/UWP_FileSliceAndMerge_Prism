using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Prism.Mvvm;
using Prism.Windows.Mvvm;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class BinaryEntiretyInfoModel:BindableBase
    {
        public StorageFile StorageFile { get; set; }
        public String FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeText
        {
            //每三位数添加一个逗号
            get { return FileSize.ToString("N0"); }
        }
        private bool isStart=false;
        public bool IsStart
        {
            get { return isStart; }
            set { SetProperty(ref isStart, value); }
        }
        private bool isDone=false;
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
        private string icon= "\xE160";
        public string Icon
        {
            get { return icon; }
            set { SetProperty(ref icon, value); }
        }
        private int sliceNumber=0;
        public int SliceNumber
        {
            get { return sliceNumber; }
            set { SetProperty(ref sliceNumber, value); }
        }
        private int sliceCompletedNumber=0;
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
    }
}

using Windows.ApplicationModel.DataTransfer;

namespace UWP_FileSliceAndMerge_Prism.Models
{
    public class DragDropData
    {
        public DataPackageOperation AcceptedOperation { get; set; }

        public DataPackageView DataView { get; set; }
    }
}

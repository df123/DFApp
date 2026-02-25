using DFApp.CommonDtos;

namespace DFApp.Bookkeeping.Expenditure
{
    public class GetExpendituresRequestDto : FilterAndPagedAndSortedResultRequestDto
    {
        public long? CategoryId { get; set; }
        
        public bool? IsBelongToSelf { get; set; }
    }
}

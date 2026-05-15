namespace dakg.shared
{
    public class GetInventoryRequest : RequestBase
    {
        public GetInventoryRequest() : base((int)MessageId.GET_INVENTORY_REQUEST) { }

        public long Uid { get; set; }
    }

    public class GetInventoryResponse : ResponseBase
    {
        public GetInventoryResponse() : base((int)ResponseResult.SUCCESS) { }

        public List<InventoryItem> Items { get; set; } = [];
    }

    public class InventoryItem
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
    }
}

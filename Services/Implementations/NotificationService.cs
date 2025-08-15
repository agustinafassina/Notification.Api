
using NotificationApi.Services.Dto;
using NotificationApi.Services.Interfaces;

namespace NotificationApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly List<ItemDto> _items = new();

        public NotificationService()
        {
            _items.Add(new ItemDto { Id = 1, Name = "Item 1" });
            _items.Add(new ItemDto { Id = 2, Name = "Item 2" });
        }

        public IEnumerable<ItemDto> GetAllItems()
        {
            return _items;
        }

        public ItemDto GetItemById(int id)
        {
            return _items.FirstOrDefault(i => i.Id == id);
        }

        public ItemDto CreateItem(ItemCreateDto newItem)
        {
            int newId = _items.Max(i => i.Id) + 1;
            ItemDto item = new ItemDto { Id = newId, Name = newItem.Name };
            _items.Add(item);
            return item;
        }
    }
}
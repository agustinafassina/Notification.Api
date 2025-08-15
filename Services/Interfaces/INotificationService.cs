using NotificationApi.Services.Dto;

namespace NotificationApi.Services.Interfaces
{
    public interface INotificationService
    {
        IEnumerable<ItemDto> GetAllItems();
        ItemDto GetItemById(int id);
        ItemDto CreateItem(ItemCreateDto newItem);
    }
}
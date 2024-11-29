using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        void RemovePhoto(Photo photo);

        Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();

        Task<Photo> GetPhotoById(int id);
    }
}
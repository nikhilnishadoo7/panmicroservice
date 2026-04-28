using PAN.API.Domain.Entities;
using System.Threading.Tasks;

namespace PAN.API.Infrastructure.Repositories.Interfaces;

public interface IRawResponseRepository
{
    Task InsertAsync(PanResponseJson entity);
}
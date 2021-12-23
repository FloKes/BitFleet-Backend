using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitFleet.Domain;
using BitFleet.Services.DTOs;

namespace BitFleet.Services.Interfaces
{
    public interface IMalfunctionService
    {
        Task<bool> CreateMalfunctionAsync(Malfunction malfunction);

        Task<List<MalfunctionDto>> GetAllMalfunctionsAsync();

        Task<Malfunction> GetMalfunctionByIdAsync(Guid malfunctionId);

        Task<MalfunctionDto> UpdateMalfunctionAsync(Malfunction malfunction);

        Task<bool> DeleteMalfunctionAsync(Guid malfunctionId);

        Task<MalfunctionDto> GenerateMalfunctionDtoAsync(Malfunction malfunction);
    }
}
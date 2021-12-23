using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MalfunctionController : Controller
    {
        private readonly IMalfunctionService _malfunctionService;
        private readonly IMapper _mapper;


        public MalfunctionController(IMalfunctionService malfunctionService, IMapper mapper)
        {
            _malfunctionService = malfunctionService;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Malfunctions.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var malfunctionDtoList = await _malfunctionService.GetAllMalfunctionsAsync();

            return Ok(_mapper.Map<List<MalfunctionResponse>>(malfunctionDtoList));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Malfunctions.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid malfunctionId)
        {
            var malfunction = await _malfunctionService.GetMalfunctionByIdAsync(malfunctionId);
            var malfunctionDto = await _malfunctionService.GenerateMalfunctionDtoAsync(malfunction);

            return Ok(_mapper.Map<MalfunctionResponse>(malfunctionDto));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPatch(ApiRoutes.Malfunctions.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid malfunctionId, [FromBody] UpdateMalfunctionRequest request)
        {
            var malfunction = await _malfunctionService.GetMalfunctionByIdAsync(malfunctionId);
            if (malfunction == null)
            {
                return NotFound();
            }

            malfunction.IsActive = request.IsActive;
            malfunction.RepairDescription = request.RepairDescription;
            malfunction.RepairCost = request.RepairCost;

            var updatedMalfunctionDto = await _malfunctionService.UpdateMalfunctionAsync(malfunction);

            if (updatedMalfunctionDto == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<MalfunctionResponse>(updatedMalfunctionDto));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete(ApiRoutes.Malfunctions.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid malfunctionId)
        {
            var deleted = await _malfunctionService.DeleteMalfunctionAsync(malfunctionId);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
        }
        
    }
}
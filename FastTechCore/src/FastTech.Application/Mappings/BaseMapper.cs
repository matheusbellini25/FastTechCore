﻿using AutoMapper;
using FastTech.Domain.Entities;
using DTO = FastTech.Application.DataTransferObjects;

namespace FastTech.Application.Mappings;

public class BaseMapper : Profile
{
    public BaseMapper()
    {
        CreateMap<DTO.BaseModel, BaseEntity>()
            .ForMember(dest => dest.Id, opt =>
            {
                opt.Condition(src => src.Id.HasValue == false || src.Id.Value == default);
                opt.UseDestinationValue();
            })
            .ForMember(dest => dest.Id, opt =>
            {
                opt.Condition(src => src.Id.HasValue == true && src.Id.Value != default);
                opt.MapFrom(src => src.Id);
            });
    }
}

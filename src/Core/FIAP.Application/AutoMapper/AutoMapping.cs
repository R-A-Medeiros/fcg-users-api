using AutoMapper;
using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Entities;

namespace FIAP.Application.AutoMapper;

public class AutoMapping : Profile
{
    public AutoMapping()
    {
        RequestToEntity();
        //EntityToResponse();
    }

    private void RequestToEntity()
    {
        CreateMap<RequestRegisterUserJson, User>()
            .ForMember(dest => dest.Password, config => config.Ignore());

    }

    private void EntityToResponse()
    {
        // implementar caso a API cresça futuramente 
    }
}

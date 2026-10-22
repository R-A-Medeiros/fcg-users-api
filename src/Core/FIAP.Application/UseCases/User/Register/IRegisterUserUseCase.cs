using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;

namespace FIAP.Application.UseCases.User.Register;

public interface IRegisterUserUseCase
{
    Task<ResponseRegisteredUserJson> Execute(RequestRegisterUserJson request);
}

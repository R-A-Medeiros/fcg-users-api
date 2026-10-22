using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;

namespace FIAP.Application.UseCases.Login.DoLogin;

public interface IDoLoginUseCase
{
    Task<ResponseRegisteredUserJson> Execute(RequestLoginJson request);
}

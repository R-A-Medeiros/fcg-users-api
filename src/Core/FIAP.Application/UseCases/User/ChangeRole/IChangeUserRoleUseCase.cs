using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;

namespace FIAP.Application.UseCases.User.ChangeRole;

public interface IChangeUserRoleUseCase
{
    Task<ResponseUserRoleJson> Execute(Guid userIdentifier, RequestChangeRoleJson request);
}

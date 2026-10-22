using FIAp.Exception.ExceptionsBase;
using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;
using FIAP.Domain.Enums;
using FIAP.Domain.Repositories;

namespace FIAP.Application.UseCases.User.ChangeRole;

public class ChangeUserRoleUseCase : IChangeUserRoleUseCase
{
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] ValidRoles = [Roles.ADMIN, Roles.TEAM_MEMBER, Roles.USER];

    public ChangeUserRoleUseCase(
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserWriteOnlyRepository userWriteOnlyRepository,
        IUnitOfWork unitOfWork)
    {
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteOnlyRepository = userWriteOnlyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseUserRoleJson> Execute(Guid userIdentifier, RequestChangeRoleJson request)
    {
        Validate(request);

        var user = await _userReadOnlyRepository.GetUserByIdentifier(userIdentifier)
            ?? throw new NotFoundException("User not found.");

        user.Role = request.Role;

        _userWriteOnlyRepository.Update(user);
        await _unitOfWork.Commit();

        return new ResponseUserRoleJson
        {
            UserIdentifier = user.UserIdentifier,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }

    private static void Validate(RequestChangeRoleJson request)
    {
        if (string.IsNullOrWhiteSpace(request.Role) || !ValidRoles.Contains(request.Role))
        {
            throw new ErrorOnValidationException(
                [$"Invalid role. Allowed values: {string.Join(", ", ValidRoles)}."]);
        }
    }
}

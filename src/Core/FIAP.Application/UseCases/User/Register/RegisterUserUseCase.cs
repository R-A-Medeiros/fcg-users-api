using AutoMapper;
using FCG.Contracts.Events;
using FIAp.Exception.ExceptionsBase;
using FIAP.Domain.Communication.Requests;
using FIAP.Domain.Communication.Responses;
using FIAP.Domain.Repositories;
using FIAP.Domain.Security.Cryptography;
using FIAP.Domain.Security.Tokens;
using MassTransit;

namespace FIAP.Application.UseCases.User.Register;

public class RegisterUserUseCase : IRegisterUserUseCase
{
    private readonly IMapper _mapper;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserWriteOnlyRepository _userWriteOnlyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessTokenGenerator _tokenGenerator;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserUseCase(
       IMapper mapper,
       IPasswordEncripter passwordEncripter,
       IUserReadOnlyRepository userReadOnlyRepository,
       IUserWriteOnlyRepository userWriteOnlyRepository,
       IUnitOfWork unitOfWork,
       IAccessTokenGenerator tokenGenerato,
       IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _passwordEncripter = passwordEncripter;
        _userReadOnlyRepository = userReadOnlyRepository;
        _userWriteOnlyRepository = userWriteOnlyRepository;
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerato;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ResponseRegisteredUserJson> Execute(RequestRegisterUserJson request)
    {
        await Validate(request);

        var user = _mapper.Map<Domain.Entities.User>(request);
        user.Password = _passwordEncripter.Encrypt(request.Password);
        user.UserIdentifier = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        await _userWriteOnlyRepository.Add(user);
        await _unitOfWork.Commit();

        // Start the welcome-email flow: NotificationsAPI consumes this event.
        await _publishEndpoint.Publish(new UserCreatedEvent
        {
            UserId = user.UserIdentifier,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        });

        return new ResponseRegisteredUserJson
        {
            Name = user.Name,
            Token = _tokenGenerator.Generate(user)
        };
    }

    private async Task Validate(RequestRegisterUserJson request)
    {
        var result = new RegisterUserValidator().Validate(request);

        var emailExist = await _userReadOnlyRepository.ExistActiveUserWithEmail(request.Email);
        if (emailExist)
        {
            result.Errors.Add(new FluentValidation.Results.ValidationFailure("Email", "Email already registered."));
        }

        if (result.IsValid == false)
        {
            var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();

            throw new ErrorOnValidationException(errorMessages);
        }
    }
}

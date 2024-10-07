namespace MvcWithIdentityAndEFCore.Services;
public class ServiceResult<T> 
{
    public T Data { get; private init;}
    public bool WasSuccess { get; private init;}
    public string Message { get; private init;}
    private ServiceResult(bool wasSuccess, T data, string message = "")
    {
        Data = data;
        WasSuccess = wasSuccess;
        Message = message;
    }
    
    public static ServiceResult<T> Succeeded(T data, string message = "")
    {
        return new ServiceResult<T>(true, data, message);
    }

    public static ServiceResult<T> Failed(string message = "")
    {
        return new ServiceResult<T>(false, default!, message);
    }

}
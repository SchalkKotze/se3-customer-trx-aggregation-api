namespace aggregate_api.Application.Domain.Models;

public class ResponseModel<TModel> : ResponseModel
{
    public TModel Data { get; set; }
    
    public ResponseModel(TModel data)
    {
        Data = data;
    }
}

public class ResponseModel
{
    public bool IsValid => !Errors.Any() && !Exceptions.Any();
    public List<string> Errors { get; }
    public List<string> Exceptions { get; }

    public ResponseModel()
    {
        Errors = new List<string>();
        Exceptions = new List<string>();
    }

    public void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        Errors.Add(error);
    }
    public void AddException(string exception)
    {
        if (string.IsNullOrEmpty(exception)) return;

        Exceptions.Add(exception);
    }
    public void ClearErrors()
    {
        Errors.Clear();
        Exceptions.Clear();
    }

    public void MergeResponses(ResponseModel responseModel)
    {
        if (responseModel.Errors.Any())
        {
            Errors.AddRange(responseModel.Errors);
        }

        if (responseModel.Exceptions.Any())
        {
            Exceptions.AddRange(responseModel.Exceptions);
        }
    }

    public TModel MergeResponseAndReturnEntity<TModel>(ResponseModel<TModel> responseModel)
    {
        MergeResponses(responseModel);

        return responseModel.Data;
    }
}
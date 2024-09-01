using System.Collections.Generic;
using System;


public interface IModel
{
}

public interface IKeyModel : IModel
{
    public string KEY { get; set; }
}

public interface IListModel<TData> : IModel
{
    List<TData> _UNUSED_LIST_ { get; set; }
}

public interface IListToDicConverterModel : IModel
{
    void ConvertListToDictionary();
}

public interface IPostProcessAfterLoadModel : IModel
{
    void PostProcessAfterLoad();
}
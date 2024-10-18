namespace Domain.Primitives;

public interface IDeletable
{
    public bool IsDeleted { get;}

    public void Delete();

    public void UnDelete();



}
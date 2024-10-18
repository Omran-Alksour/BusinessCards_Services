using System.Linq.Expressions;

namespace Domain.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string orderBy, string orderDirection)
    {
        // Define a parameter expression representing the entity type T
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

        // Create a property expression representing the property to order by
        MemberExpression property = Expression.Property(parameter, orderBy);

        // Create a lambda expression representing a function that accesses the property
        LambdaExpression lambda = Expression.Lambda(property, parameter);

        // Determine the method name based on the order direction
        string methodName = orderDirection == "desc" ? "OrderByDescending" : "OrderBy";

        // Create a method call expression for the specified ordering method
        MethodCallExpression methodCallExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,  // Pass the existing query expression
            lambda            // Pass the lambda expression as the ordering criterion
        );

        // Create a new query with the ordering applied and return it
        return query.Provider.CreateQuery<T>(methodCallExpression);
    }
}

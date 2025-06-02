// ReSharper disable NonReadonlyMemberInGetHashCode
namespace pkd_common_utils.DataObjects;

/// <summary>
/// Data object for a 2-dimensional vector.
/// </summary>
public class Vector2D
{
    /// <summary>
    /// A vector with the points [0 1].
    /// </summary>
    public static readonly Vector2D Up = new() { X = 0.0f, Y = 1.0f };

    /// <summary>
    /// A vector with the points [0 -1].
    /// </summary>
    public static readonly Vector2D Down = new() { X = 0.0f, Y = -1.0f };
    
    /// <summary>
    /// A vector with the points [-1 0].
    /// </summary>
    public static readonly Vector2D Left = new() { X = -1.0f, Y = 0.0f };
    
    /// <summary>
    /// A vector with the points [1 0].
    /// </summary>
    public static readonly Vector2D Right = new() { X = 1.0f, Y = 0.0f };
    
    /// <summary>
    /// A vector with the points [0 0].
    /// </summary>
    public static readonly Vector2D Zero = new() { X = 0.0f, Y = 0.0f };
    
    /// <summary>
    /// The x-axis point of the vector.
    /// </summary>
    public float X { get; set; }
    
    /// <summary>
    /// The y-axis point of the vector
    /// </summary>
    public float Y { get; set; }

    /// <param name="obj">The Vector2D object to compare against this one.</param>
    /// <returns>true if the X and Y properties of this object match the X and Y properties of the compared Vector2D.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Vector2D);
    }
    
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    private bool Equals(Vector2D? other)
    {
        return other != null && X.Equals(other.X) && Y.Equals(other.Y);
    }
}
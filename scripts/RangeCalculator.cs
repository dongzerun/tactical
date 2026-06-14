using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Godot.Collections;

public partial class RangeCalculator : Node
{
    [Export] public GameArea gameArea;

    public List<Vector2I> GetDirectionalRangeCells(Vector2I startPos, Vector2I direction, Vector2I rangeSize, ShapeType shape)
    {
        List<Vector2I> results = new();
        if (direction == Vector2I.Zero)
        {
            GD.Print("GetDirectionalRangeCells direction zero");
            return results;
        }

        var length = rangeSize.X;
        var width = rangeSize.Y;

        Vector2I perpDir = new Vector2I();
        int totalPoints = 0;
        int idx = 0;
        switch (shape)
        {
            case ShapeType.CIRCLE:
                return GetRangeCells(startPos, length, DistanceAlgorithm.EUCLIDEAN);
            case ShapeType.LINE:
                totalPoints = length;
                for (int i = 0; i < totalPoints; i++)
                {
                    results.Add(Vector2I.Zero);
                }
                var current = startPos;
                for (int i = 0; i < length; i++)
                {
                    results[i] = current;
                    current += direction;
                }
                break;
            case ShapeType.CONE:
                perpDir = new Vector2I(-direction.Y, direction.X);
                totalPoints = length + width*length*(length-1);
                for (int i = 0; i < totalPoints; i++)
                {
                    results.Add(Vector2I.Zero);
                }
                idx = 0;
                for (int i = 0; i < length; i++)
                {
                    var currentWidth = i * width;
                    var center = startPos + direction * i;
                    results[idx] = center;
                    idx++;
                    for (int w = 1; w < currentWidth+1; w++)
                    {
                        results[idx] = center + perpDir * w;
                        idx++;
                        results[idx] = center - perpDir * w;
                        idx++;
                    }
                }
                break;
            case ShapeType.RECTANGLE:
                perpDir = new Vector2I(-direction.Y, direction.X);
                totalPoints = length * (2*width+1);
                for (int i = 0; i < totalPoints; i++)
                {
                    results.Add(Vector2I.Zero);
                }
                idx = 0;
                for (int i = 0; i < length; i++)
                {
                    var center = startPos +direction * i;
                    results[idx] = center;
                    idx++;
                    for (int w = 1; w < width+1; w++)
                    {
                        results[idx] = center + perpDir * w;
                        idx++;
                        results[idx] = center - perpDir * w;
                        idx++;
                    }
                }
                break;
        }

        return results;
    }

    public List<Vector2I> GetRangeCells(Vector2I center, int range, DistanceAlgorithm distanceAlgorithm=DistanceAlgorithm.MANHATTAN)
    {
        List<Vector2I> results = new();
        foreach (var x in Enumerable.Range(-range, 2 * range + 1))
        {
            foreach (var y in Enumerable.Range(-range, 2 * range + 1))
            {
                float dist=0f;
                switch (distanceAlgorithm)
                {
                    case DistanceAlgorithm.MANHATTAN:
                        dist = Math.Abs(x)+Math.Abs(y);
                        break;
                    case DistanceAlgorithm.CHEBYSHEV:
                        dist= Math.Max(Math.Abs(x), Math.Abs(y));
                        break;
                    case DistanceAlgorithm.EUCLIDEAN:
                        dist = (float)Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2));
                        break;
                }

                if (dist <= range)
                {
                    results.Add(center + new Vector2I(x,y));
                }
            }
        }
        return results;
    }
}

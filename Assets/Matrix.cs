using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Matrix<T> : IEnumerable<T>
{
	private T[] _data;
    
    public Matrix(int width, int height)
    {
	    Width = width;
	    Height = height;
	    Capacity = width * height;
	    
	    _data = new T[Capacity];
    }

	public Matrix(T[,] copyFrom)
    {
	    Capacity = copyFrom.Length;
        Width = Height = Mathf.RoundToInt(Mathf.Sqrt(Capacity));

        _data = new T[Capacity];
        int val = 0;
        
        for (int i = 0; i < Width; i++)
        {
	        for (int j = 0; j < Height; j++)
	        {
		        _data[val] = copyFrom[i, j];
		        val++;
	        }
        }
    }

	public Matrix<T> Clone() {
        Matrix<T> aux = new Matrix<T>(Width, Height);
        
        for (int i = 0; i < Width; i++)
        {
	        for (int j = 0; j < Height; j++)
	        {
		        aux[i, j] = this[i,j];
	        }
        }
        return aux;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item) {
		{
	        for (int x = x0; x <= x1; x++)
	        {
		        for (int y = y0; y <= y1; y++)
		        {
			        _data[x + Height * y] = item; 
		        }
	        }

        }
    }
	
    public List<T> GetRange(int x0, int y0, int x1, int y1) {
        List<T> l = new List<T>();
        
        for (int x = x0; x < x1; x++)
        {
	        for (int y = y0; y < y1; y++)
	        {
		        l.Add(_data[x + Height * y]); 
	        }
        }
        return l;
	}
    public T this[int x, int y] 
    {
	    get
	    {
		    return _data[x + Height * y];
	    }
	    set
	    {
		    _data[x + Height * y] = value;
	    }
	}

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Capacity { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
	    for (int i = 0; i < _data.Length; i++)
		    yield return _data[i];
    }

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}

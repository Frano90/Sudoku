using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Matrix<T> : IEnumerable<T>
{
    //IMPLEMENTAR: ESTRUCTURA INTERNA- DONDE GUARDO LOS DATOS?

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
        
        Debug.Log("asdasdsa");
        for (int i = 0; i < Width; i++)
        {
	        for (int j = 0; j < Height; j++)
	        {
		        Debug.Log(this[i,j]);
		        aux[i, j] = this[i,j];
	        }
        }
        return aux;
    }

	public void SetRangeTo(int x0, int y0, int x1, int y1, T item) {
        //IMPLEMENTAR: iguala todo el rango pasado por parámetro a item
    }

    //Todos los parametros son INCLUYENTES
    public List<T> GetRange(int x0, int y0, int x1, int y1) {
        List<T> l = new List<T>();
        //IMPLEMENTAR
        return l;
	}

    //Para poder igualar valores en la matrix a algo
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

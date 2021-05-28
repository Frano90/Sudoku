using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class Sudoku : MonoBehaviour {
	public Cell prefabCell;
	public Canvas canvas;
	public Text feedback;
	public float stepDuration = 0.05f;
	[Range(1, 82)]public int difficulty = 40;

	Matrix<Cell> _board;
	Matrix<int> _createdMatrix;
    List<int> posibles = new List<int>();
	[SerializeField] int _smallSide;
	[SerializeField] int _bigSide;
    string memory = "";
    string canSolve = "";
    bool canPlayMusic = false;
    List<int> nums = new List<int>();
    private Matrix<int> _currentCorrectSudoku;


    float r = 1.0594f;
    float frequency = 440;
    float gain = 0.5f;
    float increment;
    float phase;
    float samplingF = 48000;


    void Start()
    {
        long mem = System.GC.GetTotalMemory(true);
        feedback.text = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        memory = feedback.text;
        _bigSide = _smallSide * _smallSide;
        frequency = frequency * Mathf.Pow(r, 2);
        CreateEmptyBoard();
        ClearBoard();

        
    }

    void ClearBoard() {
	    
	    _createdMatrix = new Matrix<int>(_bigSide, _bigSide);
	    
	    int val = 0;
		foreach(var cell in _board)
		{
			cell.number = 0;
			cell.locked = cell.invalid = false;
		}
    }

	void CreateEmptyBoard() {
		float spacing = 68f;
		float startX = -spacing * 4f;
		float startY = spacing * 4f;
		
		_board = new Matrix<Cell>(_bigSide, _bigSide);
		for(int x = 0; x<_board.Width; x++) {
			for(int y = 0; y<_board.Height; y++) {
                var cell = _board[x, y] = Instantiate(prefabCell);
                cell.transform.SetParent(canvas.transform, false);
				cell.transform.localPosition = new Vector3(startX + x * spacing, startY - y * spacing, 0);
			}
		}
	}

	


    void OnAudioFilterRead(float[] array, int channels)
    {
        if(canPlayMusic)
        {
            increment = frequency * Mathf.PI / samplingF;
            for (int i = 0; i < array.Length; i++)
            {
                phase = phase + increment;
                array[i] = (float)(gain * Mathf.Sin((float)phase));
            }
        }
        
    }
    void changeFreq(int num)
    {
        frequency = 440 + num * 80;
    }
    IEnumerator ShowSequence(List<Matrix<int>> seq)
	{
		int step = 0;
		int totalSteps = GetUnLockedCellAmount();
		
		Matrix<int> completedSeq = seq[seq.Count - 1];
	
		
		for (int i = 0; i < _createdMatrix.Width; i++)
		{
			for (int j = 0; j < _createdMatrix.Height; j++)
			{
				//feedback.text = "Pasos: " + step + "/" + totalSteps + " - " + memory + " - " + canSolve;	
				
				if(_board[i,j].locked) continue;
					
				var a = completedSeq[i, j];	
				
				TranslateSpecific(a, i, j);
				step++;
				yield return new WaitForSeconds(stepDuration);	
			}	
		}
	}

	void Update () {
		if(Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
            SolvedSudoku();
        else if(Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0)) 
            CreateSudoku();

		
		if(Input.GetKeyDown(KeyCode.B))
		{
			ResolveBrute();
		}
		
		
	}
	
    void SolvedSudoku()
    {
        StopAllCoroutines();
        nums = new List<int>();
        var solution = new List<Matrix<int>>();
        watchdog = 100000 * _smallSide;
        var result = RecuSolve(_createdMatrix, 0, 0, 1, solution);
        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";
        
        StartCoroutine(ShowSequence(solution));
    }

    void CreateSudoku()
    {
        StopAllCoroutines();
        nums = new List<int>();
        canPlayMusic = false;
        ClearBoard();
        List<Matrix<int>> l = new List<Matrix<int>>();
        watchdog = 100000 * _bigSide;
        GenerateValidLine(_createdMatrix, 0, 0);
        var result = RecuSolve(_createdMatrix, 0,1, 1, l);    
        
        _createdMatrix = l[l.Count-1].Clone();

        _currentCorrectSudoku = _createdMatrix;
        LockRandomCells();
        ClearUnlocked(_createdMatrix);
        TranslateAllValues(_createdMatrix);
        long mem = System.GC.GetTotalMemory(true);
        memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
        canSolve = result ? " VALID" : " INVALID";
        feedback.text = "Pasos: " + l.Count + "/" + l.Count + " - " + memory + " - " + canSolve;
    }
    
    public int watchdog = 0;
    bool RecuSolve(Matrix<int> matrixParent, int x, int y, int protectMaxDepth, List<Matrix<int>> solution)
    {
	    watchdog--;
	    if (watchdog <= 0)
	    {
		    return false;
	    }
	    if (x >= matrixParent.Width)
	    {
		    y++;
		    x = 0;
		    if (y >= matrixParent.Height)
		    {
			    feedback.text = "Pasos: " + solution.Count + "/" + solution.Count + " - " + memory + " - " + canSolve;
			    return true;
		    }
            
	    }
	    
	    if (_board[x, y].locked)
	    {
		    return RecuSolve(matrixParent, x+1, y, 1, solution);
	    }
	    
	    for (int posibleNum = 1; posibleNum <= _bigSide; posibleNum++)
	    {
		    if (CanPlaceValue(matrixParent, posibleNum, x, y))
		    {
			    Matrix<int> result = matrixParent.Clone();
			    result[x, y] = posibleNum;
			    solution.Add(result);
			    if (RecuSolve(result, x + 1, y, 1, solution))
				    return true;
			    
		    }
	    }
	    return false;
    }
    
	void GenerateValidLine(Matrix<int> mtx, int x, int y)
	{
		int[]aux = new int[_bigSide]; 
		for (int i = 0; i < _bigSide; i++) 
		{
			aux [i] = i + 1;
		}
		int numAux = 0;
		for (int j = 0; j < aux.Length; j++) 
		{
			int r = 1 + Random.Range(j,aux.Length);
			numAux = aux [r-1];
			aux [r-1] = aux [j];
			aux [j] = numAux;
		}
		for (int k = 0; k < aux.Length; k++) 
		{
			mtx [k, 0] = aux [k];	
		}
	}


	void ClearUnlocked(Matrix<int> mtx)
	{
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					mtx[j,i] = Cell.EMPTY;
			}
		}
	}

	void TheMostBruteSudokuPlayer(Matrix<int> matrixParent, List<Matrix<int>> solution)
	{
		
		Matrix<int> result = matrixParent.Clone();
		int step = 0;
		for (int i = 0; i < result.Width; i++)
		{
			for (int j = 0; j < result.Height; j++)
			{
				step++;
					
				if (_board[i, j].locked)
					continue;


				int rgn = -1;
				
				do
				{
					rgn = Random.Range(0, _bigSide) + 1;
				} while (!CanPlaceValue(result, rgn, i, j));
				
				Debug.Log(rgn + " esto entra bien?");
				result[i, j] = rgn;
				solution.Add(result);

			}
		}
	}


	void ResolveBrute()
	{
		StopAllCoroutines();
		nums = new List<int>();
		var solution = new List<Matrix<int>>();
		TheMostBruteSudokuPlayer(_createdMatrix, solution);
		watchdog = 100000;
		StartCoroutine(ShowSequence(solution));
		long mem = System.GC.GetTotalMemory(true);
		memory = string.Format("MEM: {0:f2}MB", mem / (1024f * 1024f));
		//canSolve = result ? " VALID" : " INVALID";
	}
	
	void LockRandomCells()
	{
		List<Vector2> posibles = new List<Vector2> ();
		for (int i = 0; i < _board.Height; i++) {
			for (int j = 0; j < _board.Width; j++) {
				if (!_board [j, i].locked)
					posibles.Add (new Vector2(j,i));
			}
		}
		for (int k = 0; k < (_board.Capacity + 1)-difficulty; k++) {
			int r = Random.Range (0, posibles.Count);
			_board [(int)posibles [r].x, (int)posibles [r].y].locked = true;
			posibles.RemoveAt (r);
		}
	}

    void TranslateAllValues(Matrix<int> matrix)
    {
        for (int y = 0; y < _board.Height; y++)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                _board[x, y].number = matrix[x, y];
            }
        }
    }

    void TranslateSpecific(int value, int x, int y)
    {
        _board[x, y].number = value;
    }

    void TranslateRange(int x0, int y0, int xf, int yf)
    {
        for (int x = x0; x < xf; x++)
        {
            for (int y = y0; y < yf; y++)
            {
                _board[x, y].number = _createdMatrix[x, y];
            }
        }
    }
    void CreateNew()
    {
	    _createdMatrix = new Matrix<int>(Tests.validBoards[8]);

	    TranslateAllValues(_createdMatrix);

	    LockCells();
	    
    }

    private void CheckIncorrectCellsNumber()
    {
	    for (int x = 0; x < _board.Width; x++)
	    {
		    for (int y = 0; y < _board.Height; y++)
		    {
			    if (!CanPlaceValue(_createdMatrix, _board[x, y].number, x, y))
				    _board[x, y].invalid = true;
		    }
	    }
    }

    private void LockCells()
    {
	    for (int x = 0; x < _board.Width; x++)
	    {
		    for (int y = 0; y < _board.Height; y++)
		    {
			    if (_board[x, y].number != 0) _board[x, y].locked = true;

		    }
	    }
	    
	    
    }


    bool CanPlaceValue(Matrix<int> mtx, int value, int x, int y)
    {
        List<int> fila = new List<int>();
        List<int> columna = new List<int>();
        List<int> area = new List<int>();
        List<int> total = new List<int>();

        Vector2 cuadrante = Vector2.zero;

        for (int i = 0; i < mtx.Height; i++)
        {
            for (int j = 0; j < mtx.Width; j++)
            {
                if (i != y && j == x) columna.Add(mtx[j, i]);
                else if(i == y && j != x) fila.Add(mtx[j,i]);
            }
        }



        for (int i = 1; i <= _smallSide; i++)
        {
	        if (x < _smallSide * i)
	        {
		        cuadrante.x = (i - 1) * _smallSide;
		        break;
	        }
        }
        
        for (int j = 1; j <= _smallSide; j++)
        {
	        if (y < _smallSide * j)
	        {
		        cuadrante.y = (j - 1) * _smallSide;
		        break;
	        }
        }
        
        area = mtx.GetRange((int)cuadrante.x, (int)cuadrante.y, (int)cuadrante.x + _smallSide, (int)cuadrante.y + _smallSide);
        total.AddRange(fila);
        total.AddRange(columna);
        total.AddRange(area);
        total = FilterZeros(total);

        if (total.Contains(value))
            return false;
        else
            return true;
    }


    List<int> FilterZeros(List<int> list)
    {
        List<int> aux = new List<int>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != 0) aux.Add(list[i]);
        }
        return aux;
    }

    int GetUnLockedCellAmount()
    {
	    int aux = 0;
	    foreach (var cell in _board)
	    {
		    if(cell.locked) continue;

		    aux++;
	    }

	    return aux;
    }
}

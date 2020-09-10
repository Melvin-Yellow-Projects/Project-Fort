using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
	[SerializeField]
	private int x, z;

	public int X
	{
		get
		{
			return x;
		}
	}

	public int Z
	{
		get
		{
			return z;
		}
	}

	public int Y
	{
		get
		{
			return -(X + Z);
		}
	}

	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
	}   

	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		int xCube = x;
		int zCube = z - x / 2; // undo verticle shift/offset

        return new HexCoordinates(xCube, zCube);
	}

	public override string ToString()
	{
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

	public string ToStringOnSeparateLines()
	{
		return "x:" + X.ToString() + "\ny:" + Y.ToString() + "\nz:" + Z.ToString();
		//return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

    public static HexCoordinates FromPosition(Vector3 position)
    {
		float z = position.z / (2f * HexMetrics.innerRadius);
        float y = -z;

        float offset = position.x / (3f * HexMetrics.outerRadius);
        z -= offset;
        y -= offset;

        // round values to coordinates
        int iX = Mathf.RoundToInt(-(y + z));
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(z);

        // check for rounding error
        if (iX + iY + iZ != 0)
        {
			// get each rounding delta
			float deltaX = Mathf.Abs(-(y + z) - iX);
			float deltaY = Mathf.Abs(y - iY);
			float deltaZ = Mathf.Abs(z - iZ);

			// check for largest delta
			if (deltaX > deltaY && deltaX > deltaZ)
			{
				iX = -iY - iZ; // reconstruct X
			}
			else if (deltaZ > deltaY)
			{
				iZ = -iX - iY; // reconstruct Z
			}
		}

        return new HexCoordinates(iX, iZ); // auto reconstruct Y

    }
}
sampler2D _HexCellData;
float4 _HexCellData_TexelSize;

// toggles visibility and exploration for a hex cell
float4 FilterCellData (float4 data)
{
	#if defined(HEX_MAP_EDIT_MODE)
		data.xy = 1;
	#endif
	return data;
}

// ...Give it an integer index parameter, which we use to access the cell index vector's component.
float4 GetCellData (appdata_full v, int index)
{
    // first step of constructing the U coordinate is to divide the cell index by the texture width;
    // we can do this by multiplying with _HexCellData_TexelSize.x; Because we're sampling a 
    // texture, we want to use UV coordinates that align with the centers of pixels. That ensures 
    // that we sample the correct pixels. So add ½ before dividing by the texture sizes.
    float2 uv;
	uv.x = (v.texcoord2[index] + 0.5) * _HexCellData_TexelSize.x;

    // The result is a number of the form Z.U, where Z is the row index and U is the U coordinate of
    // the cell. We can extract the row by flooring the number, then subtract that from the number
    // to get the U coordinate
    float row = floor(uv.x);
	uv.x -= row;

	// The V coordinate is found by dividing the row by the texture height; we can do this by 
	// multiplying with _HexCellData_TexelSize.y
    uv.y = (row + 0.5) * _HexCellData_TexelSize.y;

    // Now that we have the desired cell data coordinates, we can sample _HexCellData. Because we're
    // sampling the texture in the vertex program, we have to explicitly tell the shader which 
    // mipmap to use. This is done via the tex2Dlod function, which requires four texture 
    // coordinates. Because the cell data doesn't have mipmaps, set the extra coordinates to zero.
    float4 data = tex2Dlod(_HexCellData, float4(uv, 0, 0));

    // The fourth data component contains the terrain type index, which we directly stored as a 
    // byte. However, the GPU automatically converted it into a floating-point value in the 0–1 
    // range. To convert it back to its proper value, multiply it with 255.
    data.w *= 255;

    // After that, we can return the data.
	return FilterCellData(data);
}
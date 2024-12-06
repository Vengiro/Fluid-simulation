using UnityEngine;

public class Eulerian2D : MonoBehaviour
{
    public float[] verticalVelocities;
    private float[] copyVert;
    public float[] horizontalVelocities;
    private float[] copyHor;
    public bool[] vertBound;
    public bool[] horBound;
    public float[] densities;
    private float[] copyDens;
    public Vector2[] pos;

    public int width = 10;
    public int height = 10;
    public float deltaT = 0.005f;
    public float maxDensity = 100.0f;
    public float cellSize = 1.0f;
    public float restDensity = 1.0f;
    public float overRelaxation = 1.9f;
    public int divIter = 100;

    private ComputeBuffer densityBuffer;
    private ComputeBuffer positionBuffer;
    public ComputeBuffer argsBuffer;

     [Header("Rendering Data")]
    public Mesh mesh;
	public Material material;

    private  bool[] clicked;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        verticalVelocities = new float[width * (height+1)];
        copyVert = new float[width * (height+1)];
        horizontalVelocities = new float[(width+1) * height];
        copyHor = new float[(width+1) * height];
        vertBound = new bool[width * (height+1)];
        horBound = new bool[(width+1) * height];
        densities = new float[width * height];
        copyDens = new float[width * height];
        pos = new Vector2[width * height];
        clicked = new bool[width * height];
        for(int i=0; i<height+1; i++){
            for(int j=0; j<width; j++){
                verticalVelocities[j + i * width] = 0;
                copyVert[j + i * width] = 0;
                if(i==0 || i==height){
                    vertBound[j + i * width] = true;
                }
                else{
                    vertBound[j + i * width] = false;
                }
            }
        }
        for(int i=0; i<height; i++){
            for(int j=0; j<width+1; j++){
                horizontalVelocities[j + i * (width+1)] = 0;
                copyHor[j + i * (width+1)] = 0;
                if(j==0 || j==width){
                    horBound[j + i * (width+1)] = true;
                }
                else{
                    horBound[j + i * (width+1)] = false;
                }

            }
        }
        for(int i=0; i<height; i++){
            for(int j=0; j<width; j++){
                if (j < 3 && i < 2*height / 3 && i > height / 3)
            {
                densities[j + i * width] = restDensity;
            }
            else{
                densities[j + i * width] = 0;
            }
                copyDens[j + i * width] = 0;
                pos[j + i * width] = new Vector2(j, i);
            }
        }

        densityBuffer = new ComputeBuffer(width * height, sizeof(float));
        densityBuffer.SetData(densities);
        positionBuffer = new ComputeBuffer(width * height, sizeof(float) * 2);
        positionBuffer.SetData(pos);

        uint[] args = { mesh.GetIndexCount(0), (uint)(width * height), mesh.GetIndexStart(0), mesh.GetBaseVertex(0), 0};
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        material.SetBuffer("DensityBuffer", densityBuffer);
        material.SetBuffer("PositionBuffer", positionBuffer);
        material.SetFloat("_MaxDensity", maxDensity);
        material.SetFloat("_CellSize", cellSize);
        

        // Example: Initialize with random small velocities
        /**
        for (int i = 0; i < width * height; i++)
        {
            verticalVelocities[i] = Random.Range(-0.1f, 0.1f);
            horizontalVelocities[i] = Random.Range(-0.1f, 0.1f); 
        }
        */
    }

void HandleMouseClick(int button)
{
    Vector3 mouseScreenPos = Input.mousePosition;
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
    mouseWorldPos.z = 0; // Ensure it's in 2D space

    // Convert world position to grid indices
    int cellX = Mathf.FloorToInt(mouseWorldPos.x);
    int cellY = Mathf.FloorToInt(mouseWorldPos.y);

    // Check if within bounds
    if (cellX >= 0 && cellX < width && cellY >= 0 && cellY < height)
    {
        if (button == 0)
        {
            IncreaseVelocityAtCell(cellX, cellY, false);
            clicked[cellX + cellY * width] = !clicked[cellX + cellY * width];
        }
        else if (button == 1)
        {
            IncreaseVelocityAtCell(cellX, cellY, true);
            clicked[cellX + cellY * width] = !clicked[cellX + cellY * width];
        }
    }
}

    // Update is called once per frame


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick(0);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleMouseClick(1);
        }

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, new Vector3(width, height, 0)), argsBuffer, castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
        
    }

    public float updateRate = 0.1f;  // Time in seconds between updates
    private float timeSinceLastUpdate = 0f;
    void FixedUpdate()
    {
         timeSinceLastUpdate += Time.fixedDeltaTime;
        
        if (timeSinceLastUpdate >= updateRate)
        {
            for(int i=0; i<height; i++){
                for(int j=0; j<width; j++){
                    if(clicked[j + i * width]){
                        IncreaseVelocityAtCell(j, i, false);
                    }
                    if(j==0 && i < 2*height / 3 && i > height / 3){
                        densities[j + i * width] = maxDensity;
                        horizontalVelocities[j + i * (width+1)] = 15.0f;
                    }
                }
                    
            }
            advectDensity();
            advect();
            
            clearDivergence();
            
            
            densityBuffer.SetData(densities);
            material.SetBuffer("DensityBuffer", densityBuffer);
            
           
            timeSinceLastUpdate = 0f;
        }
    }

    
    void clearDivergence()
    {   
        for(int n=0; n<divIter; n++){
            for(int i=0; i<height; i++){
                for(int j=0; j<width; j++){
                    float div = verticalVelocities[j+i*width]-verticalVelocities[j+(i+1)*width] + 
                                horizontalVelocities[j+i*width]-horizontalVelocities[j+1+i*width];
                    div *= overRelaxation;
                    int vup = vertBound[j + (i+1) * width] ? 0 : 1;
                    int vdown = vertBound[j + i * width] ? 0 : 1;
                    int hleft = horBound[j + i * (width+1)] ? 0 : 1;
                    int hright = horBound[j+1 + i * (width+1)] ? 0 : 1;
                    int tot = vup+vdown+hleft+hright;


                    verticalVelocities[j+i*width]-=vdown*div/tot;
                    verticalVelocities[j+(i+1)*width]+=vup*div/tot;
                    horizontalVelocities[j+i*width]-=hleft*div/tot;
                    horizontalVelocities[j+1+i*width]+=hright*div/tot;
                }
            }
        }
    }

    float avgVert(int x, int y)
    {
        // up left, up right etc.
        int yul = (int)(Mathf.Max(x-1, 0) + (y+1)*width);
        int yur = (int)(Mathf.Min(x, width-1) + (y+1)*width);
        int ydl = (int)(Mathf.Max(x-1, 0) + y*width);
        int ydr = (int)(Mathf.Min(x, width-1) + y*width);
        return (verticalVelocities[yul]+verticalVelocities[yur]+verticalVelocities[ydl]+verticalVelocities[ydr])/4;
    }

     float avgHor(int x, int y)
    {
        // up left, up right etc.
        int xul = (int)(x + Mathf.Min(y, height-1)*(width+1));
        int xur = (int)(x+1 + Mathf.Min(y, 0, height-1)*(width+1));
        int xdl = (int)(x + Mathf.Max(y-1, 0)*(width+1));
        int xdr = (int)(x+1 + Mathf.Max(y-1, 0)*(width+1));
        return (horizontalVelocities[xul]+horizontalVelocities[xur]+horizontalVelocities[xdl]+horizontalVelocities[xdr])/4;
    }
    float bilineraInter(float x, float y, float[] data, int width, int height)
    {
        int x0 = Mathf.FloorToInt(x);
        int x1 = Mathf.Min(x0 + 1, width - 1);
        int y0 = Mathf.FloorToInt(y);
        int y1 = Mathf.Min(y0 + 1, height - 1);

        float wx = x-x0;
        float wy = y-y0;

        // Bilinear interpolation
        return 
            (1 - wx) * (1 - wy) * data[x0 + y0 * width] +
            wx * (1 - wy) * data[x1 + y0 * width] +
            (1 - wx) * wy * data[x0 + y1 * width] +
            wx * wy * data[x1 + y1 * width];
    }
    void advect()
    {   
        //vertical
        for(int i=0; i<height+1; i++){
            for(int j=0; j<width; j++){
                // In the middle of the edge x axis
                float x = j + 0.5f - deltaT*avgHor(j, i);
                float y = i - deltaT*verticalVelocities[j+i*width];

                x = Mathf.Clamp(x, 0, width-1);
                y = Mathf.Clamp(y, 0, height);

               

                // Bilinear interpolation
                copyVert[j + i * width] = bilineraInter(x, y, verticalVelocities, width, height+1);
                   
            }
        }

        //horizontal
        for(int i=0; i<height; i++){
            for(int j=0; j<width+1; j++){
                // In the middle of the edge y axis
                float x = j - deltaT*horizontalVelocities[j+i*(width+1)];
                float y = i+ 0.5f - deltaT*avgVert(j, i);

                x = Mathf.Clamp(x, 0, width);
                y = Mathf.Clamp(y, 0, height-1);

               

                // Bilinear interpolation
                copyHor[j + i * (width + 1)] = bilineraInter(x, y, horizontalVelocities, width+1, height);
                
            }
        }
        // Swap buffers
        var temp = verticalVelocities;
        verticalVelocities = copyVert;
        copyVert = temp;

        temp = horizontalVelocities;
        horizontalVelocities = copyHor;
        copyHor = temp;
        
    }

    Vector2 avgCell(int x, int y){
        Vector2 avg = new Vector2(0, 0);
        avg.x = (float)((horizontalVelocities[x + y * (width + 1)] + horizontalVelocities[x + 1 + y * (width + 1)])*0.5);
        avg.y = (float)((verticalVelocities[x + y * width] + verticalVelocities[x + (y + 1) * width])*0.5);
        return avg;
    }

    public float res;
    void advectDensity()
    {
        for(int i=0; i<height; i++){
            for(int j=0; j<width; j++){
                // In the middle of the cell
                Vector2 avg = avgCell(j, i);
                float x = j - deltaT*avg.x;
                float y = i - deltaT*avg.y;

                x = Mathf.Clamp(x, 0, width-1);
                y = Mathf.Clamp(y, 0, height-1);

               

                // Bilinear interpolation
                res = bilineraInter(x, y, densities, width, height);
                copyDens[j + i * width] = res;
            }
        }

        // Swap buffers
        var temp = densities;
        densities = copyDens;
        copyDens = temp;
    }

    void IncreaseVelocityAtCell(int x, int y, bool vert)
    {
        if(vert){
            verticalVelocities[x + y * width] += 5.0f;
        }
        else{
            horizontalVelocities[x + y * (width + 1)] += 5.0f;
        }
       
    }

    void OnDrawGizmos()
    {
        if (verticalVelocities == null || horizontalVelocities == null) return;
        if(Application.isPlaying){
            
            Gizmos.color = Color.white;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Compute position at the center of the cell
                    Vector3 position = new Vector3(j + 0.5f, i + 0.5f, 0);

                    // Compute the average velocity at the cell center
                    float averageVertical = (verticalVelocities[j + i * width]
                                        + verticalVelocities[j + (i + 1) * width]) * 0.5f;

                    float averageHorizontal = (horizontalVelocities[j + i * (width + 1)]
                                            + horizontalVelocities[j + 1 + i * (width + 1)]) * 0.5f;

                    Vector3 velocity = new Vector3(averageHorizontal, averageVertical, 0);

                    // Draw the velocity vector as an arrow
                    Vector3 arrowEnd = position + velocity * 0.5f; // Scale velocity for visibility
                    Gizmos.DrawLine(position, arrowEnd);

                    // Draw arrowhead
                    Vector3 arrowDirection = velocity.normalized * 0.1f; // Normalize for arrowhead size
                    Vector3 perpendicular = new Vector3(-arrowDirection.y, arrowDirection.x, 0); // Perpendicular vector

                    Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDirection + perpendicular * 0.5f);
                    Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDirection - perpendicular * 0.5f);

                    // Optionally draw a square for each cell
                    Gizmos.color = Color.gray;
                    Gizmos.DrawWireCube(new Vector3(j + 0.5f, i + 0.5f, 0), new Vector3(1, 1, 0));
                        
                    Gizmos.color = Color.white; // Reset color for arrows
                }
                    
            }
        }
        else{
            for(int i=0; i<height; i++){
                for(int j=0; j<width; j++){
                    // Optionally draw a square for each cell
                    Gizmos.color = Color.gray;
                    Gizmos.DrawWireCube(new Vector3(j + 0.5f, i + 0.5f, 0), new Vector3(1, 1, 0));
                    Gizmos.color = Color.blue; // Reset color for arrows
                    
                }
            }
        }
    }

    
}

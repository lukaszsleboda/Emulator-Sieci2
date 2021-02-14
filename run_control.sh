#!/bin/bash
 
 
osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./Host/bin/Debug/netcoreapp3.1/Host.dll './Config/Host/H1D1.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'

osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./Host/bin/Debug/netcoreapp3.1/Host.dll './Config/Host/H3D2.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'

osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./NCC/bin/Debug/netcoreapp3.1/NCC.dll './Config/NCC/NCC1.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'

osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./NCC/bin/Debug/netcoreapp3.1/NCC.dll './Config/NCC/NCC2.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'


osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./Control/bin/Debug/netcoreapp3.1/Control.dll './Config/Control/Control1.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'

osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./Control/bin/Debug/netcoreapp3.1/Control.dll './Config/Control/Control2.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'

osascript -e 'tell app "Terminal"
    do script " 
    cd Desktop/programowanie/cSharp/TSST/tsst_proj2
    dotnet ./Control/bin/Debug/netcoreapp3.1/Control.dll './Config/Control/Control3.json'"
    set bounds of front window to {0, 0, 500, 300}
end tell'
 
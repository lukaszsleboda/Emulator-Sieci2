for ((i=2;i<100;i++))
do
    sudo ifconfig lo0 alias 127.0.0.$i down
done
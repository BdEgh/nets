#!/bin/bash
if [ "$1" == "" ] || [ "$1" == "-h" ] || [ "$1" == "--help" ]; then
        echo "Output traceroute from given IP address with additional information"
        echo "Usage example: ./tas.sh 5.255.255.5"
        exit 0
fi

reg="[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}"
if [[ ! $1 =~ $reg ]]; then
        echo "Input correct IP address"
        exit 9
fi

printf "%5s %15s %10s %25s %4s\n" "№" "IP" "AS" "Provider" "Country"
traceroute $1 -w 0.3 -q 1 | while read i; do
        number="$(echo $i | awk '{print $1}')"
        if [[ $number =~ [0-9] ]]; then
                ip="$(echo $i | awk '{print $3}')"
                if [[ $ip =~ ($reg) ]]; then
                        ip="$(echo ${ip:1:-1})"
                        data="$(curl -s https://www.nic.ru/whois/?searchWord=$ip)"
                        name="$(echo "$data" | grep -E 'netname:' | head -n1 | awk '{print $2}')"
                        as="$(echo "$data" | grep -E 'origin:' | head -n1 | awk '{print $2}')"
                        country="$(echo "$data" | grep -E 'country:' | head -n1 | awk '{print $2}')"
                else
                        ip="HIDDEN"
                        data="UNKNOWN"
                        name="UNKNOWN"
                        as="UNKNOWN"
                        country="UN"
                fi
                printf "%5s %15s %10s %25s %4s\n" $number $ip $as $name $country
        fi
done

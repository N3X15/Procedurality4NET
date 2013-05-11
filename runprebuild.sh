#!/bin/sh

case "$1" in

  'clean')

    mono Prebuild/Prebuild.exe /clean

  ;;


  'autoclean')

    echo y|mono Prebuild/Prebuild.exe /clean

  ;;


  'vs2010')
  
    mono Prebuild/Prebuild.exe /target vs2010
  
  ;;


  'vs2008')

    mono Prebuild/Prebuild.exe /target vs2008

  ;;


  *)

    mono Prebuild/Prebuild.exe /target nant
    mono Prebuild/Prebuild.exe /target vs2008

  ;;


esac


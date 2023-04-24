using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Item 
{
    public string nome;
    public int quantidade;
    public bool gasta;
    public Texture2D imagem;
    public bool visivel;
}

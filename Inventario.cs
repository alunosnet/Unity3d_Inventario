using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{
    List<Item> inventario;
    List<Item> inventarioNaoVisivel;
    [SerializeField] GameObject panelInventario;
    Image[] imagens;
    [SerializeField] Texture2D imagemFundo;

    private IDataService _dataService = new JsonDataService();

    // Start is called before the first frame update
    void Start()
    {
        inventario = new List<Item>();
        inventarioNaoVisivel = new List<Item>();
        imagens = Utils.GetComponentsInChildWithoutRoot<Image>(panelInventario);
        Load();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Inventario"))
        {
            Debug.Log("Abre inventário");
            if (panelInventario.activeSelf == false && Time.timeScale == 1)
            {

                panelInventario.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
                ShowItems();
            }
            else
            {
                if (panelInventario.activeSelf == true && Time.timeScale == 0)
                {
                    Save();
                    panelInventario.SetActive(false);
                    Time.timeScale = 1;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }
    //Mostrar os items do inventário no panel
    void ShowItems()
    {
        //limpar inventário
        for (int i = 0; i < imagens.Length; i++)
        {
            if (imagens[i] != null)
            {
                if (imagemFundo != null)
                {
                    Rect rect = new Rect(0, 0, imagemFundo.width, imagemFundo.height);
                    imagens[i].sprite = Sprite.Create(imagemFundo, rect, new Vector2(0, 0));
                }
                else
                {
                    imagens[i].sprite = null;
                }
                imagens[i].color = new Color(imagens[i].color.r, imagens[i].color.g, imagens[i].color.g);
                imagens[i].type = Image.Type.Sliced;
                imagens[i].GetComponentInChildren<Text>().text = "";
            }
        }
        //mostrar inventário
        for (int i = 0; i < inventario.Count; i++)
        {
            Rect rect = new Rect(0, 0, inventario[i].imagem.width, inventario[i].imagem.height);
            imagens[i].sprite = Sprite.Create(inventario[i].imagem, rect, new Vector2(0, 0));
            imagens[i].color = new Color(imagens[i].color.r, imagens[i].color.g, imagens[i].color.b, 1);
            imagens[i].GetComponentInChildren<Text>().text = inventario[i].quantidade.ToString();
        }
    }

    //adiciona um item ao inventário
    public void Adicionar(Item item)
    {
        if (item.visivel)
        {
            if (inventario.Count >= imagens.Length)
            {
                SistemaMensagem.instance.MostrarMensagem("Inventário está cheio");
                return;
            }
            //verifica se o item já existe no inventário
            if (Existe(item.nome))
                AtualizaQuantidade(item);   //atualiza a quantidade
            else
            {
                inventario.Add(item);
            }
        }
        else
        {
            inventarioNaoVisivel.Add(item);
        }
    }
    //devolve verdadeiro se existe um item com o mesmo nome
    public bool Existe(string nome)
    {
        for (int i = 0; i < inventario.Count; i++)
            if (inventario[i].nome == nome) return true;

        for (int i = 0; i < inventarioNaoVisivel.Count; i++)
            if (inventarioNaoVisivel[i].nome == nome) return true;

        return false;
    }
    //atualiza a quantidade de um item do inventário
    void AtualizaQuantidade(Item item)
    {
        for (int i = 0; i < inventario.Count; i++)
        {
            Item atual = inventario[i];
            if (inventario[i].nome == item.nome)
            {
                atual.quantidade += item.quantidade;
                inventario[i] = atual;
                return;
            }
        }
    }
    public void GastaItem(string nome, int quantidade = 1)
    {
        for (int i = 0; i < inventario.Count; i++)
        {
            Item atual = inventario[i];
            if (inventario[i].nome == nome && inventario[i].gasta)
            {
                atual.quantidade -= quantidade;
                if (atual.quantidade <= 0)
                    inventario.RemoveAt(i);
                else
                    inventario[i] = atual;
                return;
            }
        }
    }

    public void Save()
    {
        List<ItemSaved> list = new List<ItemSaved>();
        foreach (Item item in inventario)
        {
            ItemSaved itemSaved = new ItemSaved();
            itemSaved.nome = item.nome;
            itemSaved.visivel = item.visivel;
            itemSaved.gasta = item.gasta;
            itemSaved.quantidade = item.quantidade;
            itemSaved.textura = SerializeText(item.imagem);
            list.Add(itemSaved);
        }
        if (_dataService.SaveData("/player-inventario.json", list, false, false))
        {
            Debug.Log("Save done!");
        }
        else
        {
            Debug.LogError("Save failed!");
        }
    }

    public void Load()
    {
        List<ItemSaved> list = new List<ItemSaved>();
        try
        {

            list = _dataService.LoadData<List<ItemSaved>>("/player-inventario.json", false);
            if (list == null)
                return;
            foreach (ItemSaved itemSaved in list)
            {

                Item item = new Item();
                inventario.Clear();
                item.nome = itemSaved.nome;
                item.visivel = itemSaved.visivel;
                item.quantidade = itemSaved.quantidade;
                item.gasta = itemSaved.gasta;
                item.imagem = DeSerializeText(itemSaved.textura);
                inventario.Add(item);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    public struct ItemSaved
    {
        public string nome;
        public int quantidade;
        public bool gasta;
        public SerializeTexture textura;
        public bool visivel;
    }
    public class SerializeTexture
    {
        [SerializeField]
        public int x;
        [SerializeField]
        public int y;
        [SerializeField]
        public byte[] bytes;
    }
    public SerializeTexture SerializeText(Texture2D tex)
    {
        SerializeTexture exportObj = new SerializeTexture();
        tex = DeCompress(tex);
        exportObj.x = tex.width;
        exportObj.y = tex.height;
        exportObj.bytes = ImageConversion.EncodeToPNG(tex);
        string text = JsonConvert.SerializeObject(exportObj);
        return exportObj;
    }
    public Texture2D DeSerializeText(SerializeTexture importObj)
    {

        Texture2D tex = new Texture2D(importObj.x, importObj.y);
        ImageConversion.LoadImage(tex, importObj.bytes);

        return tex;
    }
    public static Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}

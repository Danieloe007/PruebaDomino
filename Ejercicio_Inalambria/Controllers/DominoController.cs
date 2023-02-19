using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using Ejercicio_Inalambria.Models;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Ejercicio_Inalambria.Controllers
{
    public class DominoController : ApiController
    {

        public class FichasController : ApiController
        {

            List<FichaDto> fichasEncontradas = new List<FichaDto>();
            List<FichaDto> listaFichasXvalidar= new List<FichaDto>();
      

            [HttpPost]           
            public IHttpActionResult Domino(string token, [FromBody] JObject fichas)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    if (token == "Pru3b4Dom1noD4n13lEsp1nos4**")
                    {
                        int fichasCount = fichas.Count;
                        if (fichasCount >= 2 && fichasCount <= 6)
                        {

                            List<FichaDto> fichasList = new List<FichaDto>();                        
                            for (int i = 1; i <= fichasCount; i++)
                            {
                                string ficha = (string)fichas["ficha" + i];
                                bool valPareja= ValidaPareja(ficha);
                                if (!valPareja)
                                {
                                    return Content(HttpStatusCode.BadRequest, new
                                    {
                                        mensaje = "El formato enviado no es correcto"
                                    });
                                }
                                string[] numeros = ficha.Substring(1, ficha.Length - 2).Split('|');
                                FichaDto newFicha = new FichaDto
                                {
                                    Numero1 = int.Parse(numeros[0]),
                                    Numero2 = int.Parse(numeros[1])
                                };
                            
                                fichasList.Add(newFicha);
                            }
                
                            var resp = CompararFichas(fichasList);
                            string impResp = "";
                            if (resp.Count > 1) 
                            {
                                foreach (var ficha in resp)                                 
                                {
                                     impResp += "["+ ficha.Numero1.ToString() + "|" + ficha.Numero2+ "]";
                                }
                                return Ok(new { result = impResp });
                            }
                            else return Ok(new { result = "No tiene solución" });
                        }
                    }
                }
                return Unauthorized();
            }
            public List<FichaDto> CompararFichas(List<FichaDto> listaFichas)
            {
                listaFichasXvalidar = listaFichas;
                int countJ = 0;
                int countI = 0;
                bool fichaActualVolteada = false;

                while (countI < (listaFichas.Count * 2))
                {
                    countJ = 0;
                    FichaDto fichaActual = listaFichas[0];
                    for (int j = 1; j < listaFichas.Count; j++)
                    {
                        FichaDto fichaSiguiente = listaFichas[j];

                        if (fichaActual.Numero2 == fichaSiguiente.Numero1)
                        {
                            if ((j - countJ) > 1)
                            {
                                countJ++;
                                listaFichas = DesplazarFichaIzquierda(listaFichas, j);
                                fichaActual = listaFichas[countJ];
                                j = countJ;
                            }
                            else
                            {
                                countJ++;
                                fichaActual = listaFichas[countJ];
                            }
                        }
                        else if (fichaActual.Numero2 == fichaSiguiente.Numero2)
                        {
                            fichaSiguiente = InvertirFicha(fichaSiguiente);

                            if ((j - countJ) > 1)
                            {
                                countJ++;
                                listaFichas = DesplazarFichaIzquierda(listaFichas, j);
                                fichaActual = listaFichas[countJ];
                                j = countJ;
                            }
                            else
                            {
                                countJ++;
                                fichaActual = listaFichas[countJ];
                            }

                        }
                    }
                    if ((countJ + 1) < listaFichas.Count)
                    {
                        if (fichaActualVolteada == false)
                        {
                            listaFichas = listaFichasXvalidar;
                            listaFichas[0] = InvertirFicha(listaFichas[0]);
                            fichaActualVolteada = true;
                        }
                        else
                        {
                            FichaDto ficha = listaFichas[0];
                            listaFichas.RemoveAt(0);
                            listaFichas.Add(ficha);
                        }
                    }
                    else
                    {
                        if (listaFichas[0].Numero1 == listaFichas[listaFichas.Count - 1].Numero2) return listaFichas;
                        if (fichaActualVolteada == false)
                        {
                            listaFichas = listaFichasXvalidar;
                            listaFichas[0] = InvertirFicha(listaFichas[0]);
                            fichaActualVolteada = true;
                        }
                        else
                        {
                            FichaDto ficha = listaFichas[0];
                            listaFichas.RemoveAt(0);
                            listaFichas.Add(ficha);

                        }
                    }
                    countI++;
                }
                return fichasEncontradas;
            }
            /// <summary>
            /// El metodo invierte la posicion de una ficha
            /// </summary>
            /// <param name="ficha"></param>
            /// <returns></returns>
            public FichaDto InvertirFicha(FichaDto ficha) 
            {
                int num1Inicual = ficha.Numero1;
                ficha.Numero1=ficha.Numero2;
                ficha.Numero2=num1Inicual;  
                return ficha;
            }           
            /// <summary>
            /// El Metodo Valida la estrucutura de la FIcha y retorna un bool dependiendo la validacion 
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public static bool ValidaPareja(string x)
            {
                try
                {
                    if (!x.StartsWith("[") || !x.EndsWith("]"))
                        return false;

                    string[] partes = x.Substring(1, x.Length - 2).Split('|');
                    if (partes.Length != 2)
                        return false;

                    int a, b;
                    if (!int.TryParse(partes[0], out a) || !int.TryParse(partes[1], out b))
                        return false;

                    return (a >= 0 && a <= 6) && (b >= 1 && b <= 6);
                }
                catch (Exception)
                {

                    return false;
                }

            }
            /// <summary>
            /// El Metodo retorna el indice de la ficha que contenga el numero a buscar.
            /// </summary>
            /// <param name="listaFichas"></param>
            /// <param name="numero"></param>
            /// <returns></returns>
            public int BuscarfichaXnumero(List<FichaDto> listaFichas, int numero)
            {
                for (int i = 0; i < listaFichas.Count; i++)
                {
                    if (listaFichas[i].Numero1 == numero || listaFichas[i].Numero2 == numero)
                    {
                        return i;
                    }
                }
                return -1;
            }
            /// <summary>
            /// Metodo para tomar la ficha segun el indice y insertarla en otra posición a la izquierda, desplazando 
            /// el resto de fichas a la derecha            
            /// </summary>
            /// <param name="listaFichas"></param>
            /// <param name="indexFicha"></param>
            /// <returns></returns>
            public List<FichaDto> DesplazarFichaIzquierda(List<FichaDto> listaFichas, int indexFicha)
            {
                if (listaFichas.Count > 1 && indexFicha != 0)
                {
                    FichaDto ficha = listaFichas[indexFicha];
                    listaFichas.RemoveAt(indexFicha);
                    listaFichas.Insert(indexFicha - 1, ficha);
                }
                return listaFichas;
            }
           
          
        }
    }

}


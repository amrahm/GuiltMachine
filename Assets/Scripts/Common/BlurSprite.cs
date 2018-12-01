using System;
using ExtensionMethods;
using UnityEngine;

public class BlurSprite : MonoBehaviour {
    private float _rSum;
    private float _gSum;
    private float _bSum;
    private float _rLastAdd;
    private float _gLastAdd;
    private float _bLastAdd;
    private float _rLastSub;
    private float _gLastSub;
    private float _bLastSub;
    private float _aSum;

    private SpriteRenderer _rend;


    private Texture2D _sourceImage; //TODO replace with _texPadded?
    private int _sourceWidth;
    private int _sourceHeight;
    private Texture2D _texPadded;
    private int _windowSize;
    public int iterations = 2;
    private int _oldIterations = int.MinValue;

    public int radius = 2;
    private int _oldRadius = int.MinValue;
    private Color[] _texPixelsPadded;

    [Tooltip("Should be twice the max radius you need. Updating this is expensive.")]
    public int padding = 4;

    private int _oldPadding;

    private int _origWidth;
    private int _origHeight;


    private void Start() {
        _rend = GetComponent<SpriteRenderer>();
        _origWidth = _rend.sprite.texture.width;
        _origHeight = _rend.sprite.texture.height;
    }

    private Texture2D PadTexture(Texture2D tex) {
        Color[] texPixels = tex.GetPixels();
        Texture2D newTex = new Texture2D(_origWidth + padding * 2, _origHeight + padding * 2);
        Color[] padded = new Color[padding * 2 * (_origWidth + _origHeight + padding * 2) + texPixels.Length];
        int j = 0;
        for(int i = 0; i < padded.Length; i++) {
            int rW = _origWidth + padding * 2;
            if(i < rW * padding || i >= padded.Length - rW * padding || i % rW < padding || i % rW >= rW - padding) {
                Color near = texPixels[j];
                padded[i] = new Color(near.r, near.g, near.b, 0);
//                padded[i] = Color.clear;
            } else {
                int rWo = _origWidth + _oldPadding * 2;
                while(j < texPixels.Length - 1 && (j < rWo * _oldPadding || j >= texPixels.Length - rWo * _oldPadding || j % rWo < _oldPadding || j % rWo >= rWo - _oldPadding)) {
                    j++;
                }

                padded[i] = texPixels[j];
                if(j < texPixels.Length - 1) j++;
            }
        }
        newTex.SetPixels(padded);
        return newTex;
    }

    private void Update() {
        if(_oldPadding != padding) {
            padding = Math.Max(0, padding);
            _texPadded = PadTexture(_rend.sprite.texture);
            _texPixelsPadded = _texPadded.GetPixels();
            _oldPadding = padding;
            _oldRadius = int.MinValue; //Force it to update
        }

        if(_oldRadius != radius || _oldIterations != iterations) {
            _oldRadius = radius;
            _oldIterations = iterations;
            Texture2D newTex = new Texture2D(_texPadded.width, _texPadded.height);
            newTex.SetPixels(_texPixelsPadded);
            newTex = Blur(newTex, radius, iterations);
            float pixel2Units = _rend.sprite.rect.width / _rend.sprite.bounds.size.x;
            _rend.sprite = Sprite.Create(newTex, new Rect(0.0f, 0.0f, newTex.width, newTex.height), new Vector2(0.5f, 0.5f), pixel2Units);
        }
    }

    private Texture2D Blur(Texture2D image, int rad, int iters) {
        _windowSize = rad * 2 + 1;
        _sourceWidth = image.width;
        _sourceHeight = image.height;

        Texture2D tex = image;

        for(int i = 0; i < iters; i++) {
            tex = OneDimensialBlur(tex, rad, true);
            tex = OneDimensialBlur(tex, rad, false);
        }

        return tex;
    }

    private Texture2D OneDimensialBlur(Texture2D image, int rad, bool horizontal) {
        _sourceImage = image;

        Texture2D blurred = new Texture2D(image.width, image.height, image.format, false);

        if(horizontal) {
            for(int imgY = 0; imgY < _sourceHeight; ++imgY) {
                ResetSum();

                for(int imgX = 0; imgX < _sourceWidth; imgX++) {
                    if(imgX == 0) {
                        for(int x = rad * -1; x <= rad; ++x)
                            AddPixel(GetPixelWithXCheck(x, imgY));
                    } else {
                        Color toExclude = GetPixelWithXCheck(imgX - rad - 1, imgY);
                        Color toInclude = GetPixelWithXCheck(imgX + rad, imgY);

                        SubstPixel(toExclude);
                        AddPixel(toInclude);
                    }

                    blurred.SetPixel(imgX, imgY, CalcPixelFromSum());
                }
            }
        } else {
            for(int imgX = 0; imgX < _sourceWidth; imgX++) {
                ResetSum();

                for(int imgY = 0; imgY < _sourceHeight; ++imgY) {
                    if(imgY == 0) {
                        for(int y = rad * -1; y <= rad; ++y)
                            AddPixel(GetPixelWithYCheck(imgX, y));
                    } else {
                        Color toExclude = GetPixelWithYCheck(imgX, imgY - rad - 1);
                        Color toInclude = GetPixelWithYCheck(imgX, imgY + rad);

                        SubstPixel(toExclude);
                        AddPixel(toInclude);
                    }

                    blurred.SetPixel(imgX, imgY, CalcPixelFromSum());
                }
            }
        }

        blurred.Apply();
        return blurred;
    }

    private Color GetPixelWithXCheck(int x, int y) {
        return x < 0 ? _sourceImage.GetPixel(0, y).WithAlpha(0) :
               x >= _sourceWidth ? _sourceImage.GetPixel(_sourceWidth - 1, y).WithAlpha(0) :
               _sourceImage.GetPixel(x, y);
    }

    private Color GetPixelWithYCheck(int x, int y) {
        return y < 0 ? _sourceImage.GetPixel(x, 0).WithAlpha(0) :
               y >= _sourceHeight ? _sourceImage.GetPixel(x, _sourceHeight - 1).WithAlpha(0) :
               _sourceImage.GetPixel(x, y);
    }

    private void AddPixel(Color pixel) {
        if(pixel.a > 0.01f) {
            _rSum += pixel.r;
            _gSum += pixel.g;
            _bSum += pixel.b;

            _rLastAdd = pixel.r;
            _gLastAdd = pixel.g;
            _bLastAdd = pixel.b;
        } else {
            _rSum += _rLastAdd;
            _gSum += _gLastAdd;
            _bSum += _bLastAdd;
        }
        _aSum += pixel.a;
    }

    private void SubstPixel(Color pixel) {
        if(pixel.a > 0.01f) {
            _rSum -= pixel.r;
            _gSum -= pixel.g;
            _bSum -= pixel.b;

            _rLastSub = pixel.r;
            _gLastSub = pixel.g;
            _bLastSub = pixel.b;
        } else {
            _rSum += _rLastSub;
            _gSum += _gLastSub;
            _bSum += _bLastSub;
        }
        _aSum -= pixel.a;
    }

    private void ResetSum() {
        _rSum = 0;
        _gSum = 0;
        _bSum = 0;
        _aSum = 0;
        _rLastAdd = 0;
        _gLastAdd = 0;
        _bLastAdd = 0;
        _rLastSub = 0;
        _gLastSub = 0;
        _bLastSub = 0;
    }

    private Color CalcPixelFromSum() {
        return new Color(_rSum / _windowSize, _gSum / _windowSize, _bSum / _windowSize, _aSum / _windowSize);
    }
}
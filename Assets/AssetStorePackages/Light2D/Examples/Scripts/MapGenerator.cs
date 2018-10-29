using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Light2D.Examples {
    public class MapGenerator : MonoBehaviour {
        private const int ChunkSize = 32;
        private readonly Dictionary<Point2, CollMapPoint> _collMapPoints = new Dictionary<Point2, CollMapPoint>();
        private readonly List<Color> _colors = new List<Color>();
        private readonly List<Color> _lightAbsorptionColors = new List<Color>();
        private readonly List<Color> _lightEmissionColors = new List<Color>();
        private readonly Dictionary<Point2, GameObject> _tiles = new Dictionary<Point2, GameObject>();
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private Camera _mainCamera;
        private float _randFirstAddX;
        private float _randFirstAddY;
        private float _randSecondAddX;
        private float _randSecondAddY;
        public GameObject ambientLightPrefab;

        public BlockSetProfile blockSet;
        public Transform container;
        public GameObject lightObstaclesPrefab;

        public GameObject meshObjectPrefab;
        public bool randomSeed;
        public int seed;

        private void Awake() {
            _mainCamera = Camera.main;

            if(randomSeed) {
                Point2 spawnPoint = Point2.Round(FindObjectOfType<Spacecraft>().MainRigidbody.position);
                do {
                    seed = Random.Range(-9999999, 9999999);
                    InitRandom();
                    _collMapPoints.Clear();
                } while(!IsGoodSpawnPoint(spawnPoint));
            } else {
                InitRandom();
            }
        }

        private void InitRandom() {
            SimpleRNG rand = new SimpleRNG(seed);
            _randFirstAddX = (rand.value - 0.5f) * 1000f;
            _randFirstAddY = (rand.value - 0.5f) * 10000;
            _randSecondAddX = (rand.value - 0.5f) * 1000f;
            _randSecondAddY = (rand.value - 0.5f) * 1000f;
        }

        private void Start() {
            foreach(Transform transf in container.Cast<Transform>().ToArray()) Util.Destroy(transf.gameObject);
            StartCoroutine(ChunkCreatorCoroutine());
            StartCoroutine(ChunkDestroyerCoroutine());
        }

        private IEnumerator ChunkCreatorCoroutine() {
            bool firstRun = true;
            while(true) {
                Vector3 camPos = _mainCamera.transform.position;
                Point2 camChunk = Point2.Round(camPos.x / ChunkSize, camPos.y / ChunkSize);
                int camHalfHeight = Mathf.CeilToInt(_mainCamera.orthographicSize / ChunkSize) + 2;
                int camHalfWidth = Mathf.CeilToInt(_mainCamera.orthographicSize * _mainCamera.aspect / ChunkSize) + 2;

                for(int x = camChunk.x - camHalfWidth; x <= camChunk.x + camHalfWidth; x++) {
                    for(int y = camChunk.y - camHalfHeight; y <= camChunk.y + camHalfHeight; y++) {
                        camPos = _mainCamera.transform.position;
                        camChunk = Point2.Round(camPos.x / ChunkSize, camPos.y / ChunkSize);

                        if(_tiles.ContainsKey(new Point2(x, y)))
                            continue;

                        GameObject chunk = GenerateChunk(x, y);
                        _tiles[new Point2(x, y)] = chunk;

                        if(!firstRun)
                            yield return null;
                    }
                    yield return null;
                }
                firstRun = false;
            }
        }

        private IEnumerator ChunkDestroyerCoroutine() {
            List<Point2> removedChunks = new List<Point2>();
            while(true) {
                removedChunks.Clear();
                foreach(KeyValuePair<Point2, GameObject> chunk in _tiles) {
                    Vector3 camPos = _mainCamera.transform.position;
                    Point2 camChunk = Point2.Round(camPos.x / ChunkSize, camPos.y / ChunkSize);
                    int camHalfHeight = Mathf.CeilToInt(_mainCamera.orthographicSize / ChunkSize) + 20;
                    int camHalfWidth = Mathf.CeilToInt(_mainCamera.orthographicSize * _mainCamera.aspect / ChunkSize) + 20;

                    Point2 pos = chunk.Key;
                    if(pos.x < camChunk.x - camHalfWidth || pos.x > camChunk.x + camHalfWidth ||
                       pos.y < camChunk.y - camHalfHeight || pos.y > camChunk.y + camHalfHeight)
                        removedChunks.Add(pos);
                }

                yield return null;

                foreach(Point2 pos in removedChunks) {
                    Vector3 camPos = _mainCamera.transform.position;
                    Point2 camChunk = Point2.Round(camPos.x / ChunkSize, camPos.y / ChunkSize);
                    int camHalfHeight = Mathf.CeilToInt(_mainCamera.orthographicSize / ChunkSize) + 2;
                    int camHalfWidth = Mathf.CeilToInt(_mainCamera.orthographicSize * _mainCamera.aspect / ChunkSize) + 2;

                    GameObject chunk = _tiles[pos];
                    if(pos.x < camChunk.x - camHalfWidth || pos.x > camChunk.x + camHalfWidth ||
                       pos.y < camChunk.y - camHalfHeight || pos.y > camChunk.y + camHalfHeight) {
                        Destroy(chunk);
                        _tiles.Remove(pos);
                        yield return null;
                    }
                }

                yield return null;
            }
        }

        private GameObject GenerateChunk(int chunkX, int chunkY) {
            return GenerateBlocksJoined(chunkX * ChunkSize, chunkY * ChunkSize);
        }

        private GameObject GenerateBlocksJoined(int xOffest, int yOffest) {
            SimpleRNG rand = new SimpleRNG(Util.Hash(seed, xOffest, yOffest));

            _vertices.Clear();
            _uvs.Clear();
            _triangles.Clear();
            _lightAbsorptionColors.Clear();
            _lightEmissionColors.Clear();

            GameObject meshObj = Instantiate(meshObjectPrefab);
            meshObj.name = "Block Mesh { X = " + xOffest / ChunkSize + "; Y = " + yOffest / ChunkSize + " }";
            Transform meshObjTransform = meshObj.transform;
            meshObjTransform.position = meshObjTransform.position.WithXy(xOffest, yOffest);
            meshObjTransform.parent = container;

            CollMapPoint[,] collMap = new CollMapPoint[ChunkSize, ChunkSize];

            for(int x = 0; x < ChunkSize; x++)
            for(int y = 0; y < ChunkSize; y++)
                collMap[x, y] = GetCollMapPoint(x + xOffest, y + yOffest);

            for(int x = 0; x < ChunkSize; x++)
            for(int y = 0; y < ChunkSize; y++) {
                BlockSetProfile.BlockInfo blockInfo = collMap[x, y].blockInfo;

                if(blockInfo.AditionalObjectPrefab != null && blockInfo.AditionalObjectProbability >= rand.value) {
                    GameObject addObj = Instantiate(blockInfo.AditionalObjectPrefab);

                    addObj.transform.parent = meshObjTransform;
                    addObj.transform.localPosition = blockInfo.AditionalObjectPrefab
                        .transform.position.WithXy(x + 0.5f, y + 0.5f);
                }

                if(blockInfo.SpriteInfo.Length == 0) {
                    Debug.LogError("Sprite Info is broken");
                    continue;
                }

                int compactInfo =
                    (SafeIndex(collMap, x, y + 1, ChunkSize, ChunkSize,
                             () => GetCollMapPoint(x + xOffest, y + yOffest))
                         .blockType == BlockSetProfile.BlockType.CollidingWall
                         ? 1
                         : 0) +
                    (SafeIndex(collMap, x + 1, y, ChunkSize, ChunkSize,
                             () => GetCollMapPoint(x + xOffest, y + yOffest))
                         .blockType == BlockSetProfile.BlockType.CollidingWall
                         ? 2
                         : 0) +
                    (SafeIndex(collMap, x, y - 1, ChunkSize, ChunkSize,
                             () => GetCollMapPoint(x + xOffest, y + yOffest))
                         .blockType == BlockSetProfile.BlockType.CollidingWall
                         ? 4
                         : 0) +
                    (SafeIndex(collMap, x - 1, y, ChunkSize, ChunkSize,
                             () => GetCollMapPoint(x + xOffest, y + yOffest))
                         .blockType == BlockSetProfile.BlockType.CollidingWall
                         ? 8
                         : 0);

                CreatePoint(x, y, blockInfo, compactInfo, false, rand);
            }

            Mesh blockMesh = new Mesh {
                vertices = _vertices.ToArray(),
                uv = _uvs.ToArray(),
                triangles = _triangles.ToArray()
            };
            blockMesh.RecalculateBounds();
            ;

            MeshFilter meshFilter = meshObj.GetComponent<MeshFilter>();
            meshFilter.mesh = blockMesh;

            MeshRenderer meshRenderer = meshObj.GetComponent<MeshRenderer>();
            Texture2D texture = blockSet.BlockInfos
                .First(bi => bi.SpriteInfo.Any(si => si != null))
                .SpriteInfo.First(ti => ti != null)
                .texture;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetTexture("_MainTex", texture);
            meshRenderer.SetPropertyBlock(mpb);

            for(int x = 0; x < ChunkSize; x++) {
                int yStart = 0;
                for(int y = 0; y < ChunkSize; y++)
                    if(collMap[x, y].blockInfo.BlockType != BlockSetProfile.BlockType.CollidingWall) {
                        if(y - yStart > 0) {
                            GameObject obj = new GameObject {layer = meshObj.layer};
                            obj.transform.parent = meshObjTransform;
                            obj.transform.localPosition = new Vector3(x, 0);
                            obj.name = "Collider x = " + x;
                            BoxCollider2D coll = obj.AddComponent<BoxCollider2D>();
                            coll.size = new Vector2(1, y - yStart);
                            coll.offset = new Vector2(0.5f, yStart + coll.size.y / 2f);
                        }
                        yStart = y + 1;
                    }
                if(ChunkSize - yStart > 0) {
                    GameObject obj = new GameObject {layer = meshObj.layer};
                    obj.transform.parent = meshObjTransform;
                    obj.transform.localPosition = new Vector3(x, 0);
                    obj.name = "Collider x = " + x;
                    BoxCollider2D coll = obj.AddComponent<BoxCollider2D>();
                    coll.size = new Vector2(1, ChunkSize - yStart);
                    coll.offset = new Vector2(0.5f, yStart + coll.size.y / 2f);
                }
            }

            GameObject lightObstaclesObject = Instantiate(lightObstaclesPrefab);
            lightObstaclesObject.transform.parent = meshObjTransform;
            lightObstaclesObject.transform.localPosition = Vector3.zero;
            //lightObstaclesObject.transform.localPosition += new Vector3(0, 0, -10);
            MeshFilter lightObstaclesMeshFilter = lightObstaclesObject.GetComponent<MeshFilter>();
            lightObstaclesMeshFilter.mesh = ChunkMeshFromColors(_lightAbsorptionColors);

            GameObject ambientLightObject = Instantiate(ambientLightPrefab);
            ambientLightObject.transform.parent = meshObjTransform;
            ambientLightObject.transform.localPosition = Vector3.zero;
            //ambientLightObject.transform.localPosition += new Vector3(0, 0, -5);
            MeshFilter ambientLightMeshFilter = ambientLightObject.GetComponent<MeshFilter>();
            ambientLightMeshFilter.mesh = ChunkMeshFromColors(_lightEmissionColors);

            return meshObj;
        }

        private Mesh ChunkMeshFromColors(List<Color> colors) {
            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();

            const float add = 0;

            for(int x = 0; x < ChunkSize; x++) {
                int yStart = 0;
                Color startC = colors[x * ChunkSize];
                for(int y = 0; y < ChunkSize; y++) {
                    Color currC = colors[y + x * ChunkSize];
                    if(currC.r != startC.r || currC.g != startC.g || currC.b != startC.b || currC.a != startC.a) {
                        int startVert = _vertices.Count;

                        _vertices.Add(new Vector3(x - add, yStart - add, 0));
                        _vertices.Add(new Vector3(x + 1 + add, yStart - add, 0));
                        _vertices.Add(new Vector3(x - add, y + add, 0));
                        _vertices.Add(new Vector3(x + 1 + add, y + add, 0));

                        _triangles.Add(startVert + 2);
                        _triangles.Add(startVert + 1);
                        _triangles.Add(startVert);
                        _triangles.Add(startVert + 1);
                        _triangles.Add(startVert + 2);
                        _triangles.Add(startVert + 3);

                        for(int i = 0; i < 4; i++)
                            _colors.Add(startC);

                        startC = currC;

                        yStart = y;
                    }
                }
                if(ChunkSize - yStart > 0) {
                    int startVert = _vertices.Count;

                    _vertices.Add(new Vector3(x - add, yStart - add, 0));
                    _vertices.Add(new Vector3(x + 1 + add, yStart - add, 0));
                    _vertices.Add(new Vector3(x - add, ChunkSize + add, 0));
                    _vertices.Add(new Vector3(x + 1 + add, ChunkSize + add, 0));

                    _triangles.Add(startVert + 2);
                    _triangles.Add(startVert + 1);
                    _triangles.Add(startVert);
                    _triangles.Add(startVert + 1);
                    _triangles.Add(startVert + 2);
                    _triangles.Add(startVert + 3);

                    for(int i = 0; i < 4; i++)
                        _colors.Add(startC);
                }
            }

            Mesh mesh = new Mesh {
                vertices = _vertices.ToArray(),
                triangles = _triangles.ToArray(),
                colors = _colors.ToArray()
            };

            return mesh;
        }

        private void CreatePoint(int x, int y, BlockSetProfile.BlockInfo blockInfo, int compactInfo,
            bool noLightEffects, SimpleRNG rand, BlockSetProfile.BlockType? isColliding = null) {
            Sprite sprite = blockInfo.SpriteInfo
                .RandomElement(ti => 1, rand);

            //if (tilingInfo == null)
            //{
            //    Debug.LogError("Tiling info not found");
            //    return;
            //}

            //var sprite = tilingInfo.Sprite;

            if(sprite == null) {
                Debug.LogError("Tiling info is broken");
                return;
            }

            CreatePoint(x, y, sprite, blockInfo, isColliding ?? blockInfo.BlockType,
                noLightEffects);
        }

        private void CreatePoint(int x, int y, Sprite sprite, BlockSetProfile.BlockInfo blockInfo,
            BlockSetProfile.BlockType isColliding, bool noLightEffect) {
            Vector2 textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Rect uvRect = sprite.textureRect;

            int startVert = _vertices.Count;
            const float add = 0.01f;
            float z = isColliding == BlockSetProfile.BlockType.CollidingWall ? 0 : 5;
            _vertices.Add(new Vector3(x - add, y - add, z));
            _vertices.Add(new Vector3(x + 1 + add, y - add, z));
            _vertices.Add(new Vector3(x - add, y + 1 + add, z));
            _vertices.Add(new Vector3(x + 1 + add, y + 1 + add, z));

            _uvs.Add(new Vector2(uvRect.xMin / textureSize.x, uvRect.yMin / textureSize.y)); // 0, 0
            _uvs.Add(new Vector2(uvRect.xMax / textureSize.x, uvRect.yMin / textureSize.y)); // 1, 0
            _uvs.Add(new Vector2(uvRect.xMin / textureSize.x, uvRect.yMax / textureSize.y)); // 0, 1
            _uvs.Add(new Vector2(uvRect.xMax / textureSize.x, uvRect.yMax / textureSize.y)); // 1, 1

            _triangles.Add(startVert + 2);
            _triangles.Add(startVert + 1);
            _triangles.Add(startVert);
            _triangles.Add(startVert + 1);
            _triangles.Add(startVert + 2);
            _triangles.Add(startVert + 3);

            //for (int i = 0; i < 4; i++)
            //{
            _lightAbsorptionColors.Add(noLightEffect ? new Color() : blockInfo.LightAbsorption);
            _lightEmissionColors.Add(noLightEffect ? new Color() : blockInfo.LightEmission);
            //}
        }

        private CollMapPoint GetCollMapPoint(int x, int y, BlockSetProfile.BlockType? colliding = null) {
            CollMapPoint cachedPoint;
            if(_collMapPoints.TryGetValue(new Point2(x, y), out cachedPoint) &&
               (colliding == null || colliding.Value == cachedPoint.blockType))
                return cachedPoint;

            float noise = GetNoise(x, y);

            List<BlockSetProfile.BlockInfo> matchingBlockInfos = blockSet.BlockInfos.FindAll(b =>
                b.MinNoise <= noise && b.MaxNoise >= noise &&
                (colliding == null || colliding.Value == b.BlockType));

            if(matchingBlockInfos.Count == 0 && colliding != null)
                matchingBlockInfos = blockSet.BlockInfos
                    .FindAll(b => colliding.Value == b.BlockType);

            if(matchingBlockInfos.Count == 0) {
                Debug.LogError("No matching blocks found");
                return default(CollMapPoint);
            }

            SimpleRNG rand = new SimpleRNG(Util.Hash(seed, x, y));
            BlockSetProfile.BlockInfo blockInfo = matchingBlockInfos
                .RandomElement(bi => bi.Weight, rand);

            CollMapPoint point = new CollMapPoint(noise) {blockInfo = blockInfo, blockType = blockInfo.BlockType};
            _collMapPoints[new Point2(x, y)] = point;
            return point;
        }

        private float GetNoise(int x, int y) {
            float noise1 = (Noise.Generate(
                                x * blockSet.FirstNoiseScale + _randFirstAddX,
                                y * blockSet.FirstNoiseScale + _randFirstAddY) - 0.5f) * 2f;
            float noise2 = (Noise.Generate(
                                x * blockSet.SecondNoiseScale + _randSecondAddX,
                                y * blockSet.SecondNoiseScale + _randSecondAddY) - 0.5f) * 2f;

            float noise = Mathf.Clamp01((noise1 + noise2 * blockSet.SecondNoiseMul) / 2f + 0.5f);

            return noise;
        }

        private bool IsGoodSpawnPoint(Point2 point) {
            for(int x = point.x - 10; x <= point.x + 10; x++)
            for(int y = point.y - 10; y < point.y + 10; y++)
                if(GetCollMapPoint(x, y).blockType == BlockSetProfile.BlockType.CollidingWall)
                    return false;
            return true;
        }

        [ContextMenu("Recreate map")]
        private void RecreateMap() {
            foreach(KeyValuePair<Point2, GameObject> tile in _tiles) Destroy(tile.Value);
            _tiles.Clear();
        }

        private T SafeIndex<T>(T[,] arr, int x, int y, int w, int h, Func<T> noneFunc) {
            if(x < 0 || y < 0 || x >= w || y >= h) return noneFunc();
            return arr[x, y];
        }

        private struct CollMapPoint {
            public float noise;
            public BlockSetProfile.BlockInfo blockInfo;
            public BlockSetProfile.BlockType blockType;

            public CollMapPoint(float noise) {
                this.noise = noise;
                blockInfo = null;
                blockType = BlockSetProfile.BlockType.Empty;
            }
        }
    }
}
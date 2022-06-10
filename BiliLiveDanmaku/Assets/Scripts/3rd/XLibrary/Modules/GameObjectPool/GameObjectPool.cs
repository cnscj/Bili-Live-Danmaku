using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace XLibGame
{
    public class GameObjectPool : MonoBehaviour
    {
        public enum ActiveOperate
        {
            Enabled,        //失活
            Sight,          //视野
            Custom,         //自定义
        }
        /// <summary>
        /// 所属对象池管理器
        /// </summary>
        public GameObjectPoolManager mgrObj;
        /// <summary>
        /// 每个对象池的名称，当唯一id
        /// </summary>
        public string poolName;
        /// <summary>
        /// 对象预设
        /// </summary>
        public GameObject prefab;
        /// <summary>
        /// 如超过gc时间仍空闲,则移除池
        /// </summary>
        public float stayTime = 60f;
        /// <summary>
        /// 对象池中存放最大数量
        /// </summary>
        public int maxCount = 20;
        /// <summary>
        /// 对象池中存放最小数量
        /// </summary>
        public int minCount = 0;
        /// <summary>
        /// 默认初始容量
        /// </summary>
        public int defaultCount = 0;
        /// <summary>
        /// 满池时不会生成
        /// </summary>
        public bool fixedSize = false;
        /// <summary>
        ///释放模式
        /// </summary>
        public ActiveOperate activeOperate = ActiveOperate.Enabled;
        public Action<GameObject, bool> activeCustomFunc;

        /// <summary>
        /// 队列，存放对象池中没有用到的对象，即可分配对象
        /// </summary>
        protected LinkedList<GameObjectPoolObject> m_queue;
        protected float m_startTick;
        protected int m_totalCount;
        protected int m_disposeTimes;

        public GameObjectPool()
        {
            m_queue = new LinkedList<GameObjectPoolObject>();
            m_totalCount = 0;
        }

        public GameObjectPoolObject GetPoolObject(float lifeTime = 0)
        {
            UpdateTick();

            bool isAlreadyInPool = false;
            GameObjectPoolObject poolObj;
            if (m_queue.Count > 0)
            {
                //池中有待分配对象
                poolObj = m_queue.Last.Value;
                m_queue.RemoveLast();
                isAlreadyInPool = true;
            }
            else
            {
                if (prefab == null) return null;
                if (fixedSize) return null;
                    
                //池中没有可分配对象了，新生成一个
                poolObj = CreatePoolObject();
                if (poolObj == null)
                    return null;
            }

            poolObj.lifeTime = lifeTime;
            poolObj.postTimes = m_disposeTimes;
            poolObj.ownPool = this;
            
            poolObj.UpdateTick();

            var returnObj = poolObj.gameObject;
            SetGameObjectActive(returnObj, isAlreadyInPool);
            poolObj.TryCountDown();

            return poolObj;
        }

        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="lifeTime">对象存在的时间</param>
        /// <returns>生成的对象</returns>
        public GameObject GetOrCreate(float lifeTime = 0)
        {
            var poolObj = GetPoolObject(lifeTime);
            if (poolObj != null)
            {
                return poolObj.gameObject;
            }
            else
            {
                if (prefab != null)
                {
                    return Instantiate(prefab);
                }
            }
            return null;
        }

        /// <summary>
        /// “删除对象”放入对象池
        /// </summary>
        /// <param name="obj">对象</param>
        public void Release(GameObject gobj)
        {
            if (gobj == null)
                return;
      
            if (m_queue.Count > maxCount)
            {
                //当前池中object数量已满，直接销毁
                Object.Destroy(gobj);
                return;
            }

            GameObjectPoolObject poolObj = gobj.GetComponent<GameObjectPoolObject>();
            if (poolObj != null)
            {
                if (poolObj.postTimes < m_disposeTimes)
                {
                    Object.Destroy(gobj);
                    return;
                }
            }
            else
            {
                poolObj = CreatePoolObject(gobj);
            }
            poolObj.UpdateTick();

            //放入对象池，入队
            m_queue.AddLast(poolObj);
            m_totalCount = Mathf.Max(m_totalCount, m_queue.Count);

            gobj.transform.SetParent(transform, false); //不改变Transform
            SetGameObjectDeactive(gobj);
            
            UpdateTick();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            while(m_queue.Count > 0)
            {
                var go = m_queue.Last.Value;
                m_queue.RemoveLast();

                Object.Destroy(go);
            }
            m_disposeTimes++;
        }

        /// <summary>
        /// 销毁该对象池
        /// </summary>
        public void Destroy()
        {
            mgrObj?.DestroyPool(poolName);
        }

        /// <summary>
        /// 根据池原有初始化
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < defaultCount && i < maxCount; i++)
            {
                if (i < transform.childCount)
                {
                    GameObject availableGameObject = transform.GetChild(i).gameObject;
                    Release(availableGameObject);   //放回池中待利用
                }
                else
                {
                    if (prefab != null)
                    {
                        GameObject availableGameObject = Object.Instantiate(prefab);
                        Release(availableGameObject);   //放回池中待利用
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private GameObjectPoolObject CreatePoolObject(GameObject go = null)
        {
            GameObject returnObj = null;
            if (go != null)
            {
                returnObj = go;
            }
            else
            {
                if (prefab != null)
                {
                    returnObj = Instantiate(prefab);
                }else
                {
                    return null;
                }
            }
            returnObj.transform.SetParent(gameObject.transform);
            returnObj.SetActive(false);

            //使用PrefabInfo脚本保存returnObj的一些信息
            GameObjectPoolObject poolObj = returnObj.GetComponent<GameObjectPoolObject>();
            if (poolObj == null)
            {
                poolObj = returnObj.AddComponent<GameObjectPoolObject>();
            }
            return poolObj;
        }

        private void UpdateTick()
        {
            m_startTick = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 将自己加入到对象池管理中去
        /// </summary>
        private void Awake()
        {
            Init();
            UpdateTick();
        }

        /// <summary>
        // 移除掉无效的自己
        /// </summary>
        private void Start()
        {
            if (string.IsNullOrEmpty(poolName))
            {
                Destroy();
            }
        }

        /// <summary>
        /// 被销毁清空自己
        /// </summary>
        private void OnDestroy()
        {
            m_queue.Clear();
        }

        private void Update()
        {
            UpdatePool();
            UpdatePoolObjects();
        }

        private void SetGameObjectActive(GameObject gobj, bool isAlreadyInPool)
        {
            if (gobj == null)
                return;

            switch (activeOperate)
            {
                case ActiveOperate.Enabled:
                    gobj.SetActive(true);
                    break;
                case ActiveOperate.Sight:
                    {
                        if (isAlreadyInPool)
                        {
                            var position = gobj.transform.position;
                            position.z -= -1000f;
                            gobj.transform.position = position;
                        }
                        else
                        {
                            gobj.SetActive(true);
                        }
                    }
                    break;
                case ActiveOperate.Custom:
                    activeCustomFunc?.Invoke(gobj, true);
                    break;
            }
        }

        private void SetGameObjectDeactive(GameObject gobj)
        {
            if (gobj == null)
                return;

            switch (activeOperate)
            {
                case ActiveOperate.Enabled:
                    gobj.SetActive(false);
                    break;
                case ActiveOperate.Sight:
                    {
                        var position = gobj.transform.position;
                        position.z += -1000f;
                        gobj.transform.position = position;
                    }
                    break;
                case ActiveOperate.Custom:
                    activeCustomFunc?.Invoke(gobj, false);
                    break;
            }
        }

        private void UpdatePool()
        {
            if (stayTime > 0f)
            {
                if (m_queue.Count >= m_totalCount)  //XXX:这里有可能不还回池
                {
                    if (m_startTick + stayTime <= Time.realtimeSinceStartup)
                    {
                        Destroy();
                    }
                }
                else
                {
                    UpdateTick();
                }

            }
        }

        private void UpdatePoolObjects()
        {
            if (m_queue.Count <= minCount)
                return;

            for (LinkedListNode<GameObjectPoolObject> iterNode = m_queue.Last; iterNode != null; iterNode = iterNode.Previous)
            {
                var poolObj = iterNode.Value;
                if (poolObj != null)
                {
                    if (poolObj.CheckTick())
                    {
                        var returnObj = poolObj.gameObject;
                        Object.Destroy(returnObj);
                        m_queue.Remove(iterNode);
                    }
                }
            }
        }
    }
}

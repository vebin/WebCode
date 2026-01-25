// IndexedDB 存储管理 - 用于会话历史持久化
console.log('🔧 indexeddb-storage.js 正在加载...');

// 添加全局标志，表示 IndexedDB 是否已准备就绪
window.webCliIndexedDBReady = false;

/**
 * IndexedDB 会话存储管理器
 */
window.webCliIndexedDB = (function() {
    const DB_NAME = 'WebCliDB';
    const DB_VERSION = 4; // 升级到版本 4，添加性能指标、快捷操作、设置存储
    const STORE_NAME = 'sessions';
    const TEMPLATE_STORE = 'templates';
    const HISTORY_STORE = 'input_history';
    const OUTPUT_STORE = 'session_outputs';
    const METRICS_STORE = 'performance_metrics'; // 性能指标存储
    const QUICK_ACTIONS_STORE = 'quick_actions'; // 快捷操作存储
    const SETTINGS_STORE = 'settings'; // 通用设置存储
    const LEGACY_STORAGE_KEY = 'webcli_sessions';
    
    let dbInstance = null;
    let initPromise = null;

    /**
     * 初始化 IndexedDB
     * @returns {Promise<IDBDatabase>}
     */
    async function initDB() {
        if (dbInstance) {
            return dbInstance;
        }

        if (initPromise) {
            return initPromise;
        }

        initPromise = new Promise((resolve, reject) => {
            const request = indexedDB.open(DB_NAME, DB_VERSION);

            request.onerror = () => {
                console.error('❌ IndexedDB 打开失败:', request.error);
                reject(request.error);
            };

            request.onsuccess = () => {
                dbInstance = request.result;
                console.log('✅ IndexedDB 初始化成功');
                resolve(dbInstance);
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                const oldVersion = event.oldVersion;
                console.log(`🔄 IndexedDB 升级中... (从版本 ${oldVersion} 到版本 ${DB_VERSION})`);

                // 创建会话存储（如果不存在）
                if (!db.objectStoreNames.contains(STORE_NAME)) {
                    const objectStore = db.createObjectStore(STORE_NAME, { keyPath: 'sessionId' });
                    
                    // 创建索引
                    objectStore.createIndex('updatedAt', 'updatedAt', { unique: false });
                    objectStore.createIndex('createdAt', 'createdAt', { unique: false });
                    
                    console.log('✅ 会话对象存储创建成功');
                }

                // V2: 创建模板存储
                if (!db.objectStoreNames.contains(TEMPLATE_STORE)) {
                    const templateStore = db.createObjectStore(TEMPLATE_STORE, { keyPath: 'id' });
                    
                    // 创建索引
                    templateStore.createIndex('category', 'category', { unique: false });
                    templateStore.createIndex('createdAt', 'createdAt', { unique: false });
                    templateStore.createIndex('isCustom', 'isCustom', { unique: false });
                    templateStore.createIndex('isFavorite', 'isFavorite', { unique: false });
                    
                    console.log('✅ 模板对象存储创建成功');
                }

                // V2: 创建输入历史存储
                if (!db.objectStoreNames.contains(HISTORY_STORE)) {
                    const historyStore = db.createObjectStore(HISTORY_STORE, { keyPath: 'id', autoIncrement: true });
                    
                    // 创建索引
                    historyStore.createIndex('timestamp', 'timestamp', { unique: false });
                    historyStore.createIndex('text', 'text', { unique: false });
                    
                    console.log('✅ 输入历史对象存储创建成功');
                }

                // V3: 创建输出结果存储（按 sessionId 保存输出 Tab 状态）
                if (!db.objectStoreNames.contains(OUTPUT_STORE)) {
                    const outputStore = db.createObjectStore(OUTPUT_STORE, { keyPath: 'sessionId' });
                    outputStore.createIndex('updatedAt', 'updatedAt', { unique: false });
                    console.log('✅ 输出结果对象存储创建成功');
                }

                // V4: 创建性能指标存储
                if (!db.objectStoreNames.contains(METRICS_STORE)) {
                    const metricsStore = db.createObjectStore(METRICS_STORE, { keyPath: 'id', autoIncrement: true });
                    metricsStore.createIndex('timestamp', 'timestamp', { unique: false });
                    metricsStore.createIndex('operation', 'operation', { unique: false });
                    console.log('✅ 性能指标对象存储创建成功');
                }

                // V4: 创建快捷操作存储
                if (!db.objectStoreNames.contains(QUICK_ACTIONS_STORE)) {
                    const quickActionsStore = db.createObjectStore(QUICK_ACTIONS_STORE, { keyPath: 'id' });
                    quickActionsStore.createIndex('order', 'order', { unique: false });
                    console.log('✅ 快捷操作对象存储创建成功');
                }

                // V4: 创建通用设置存储
                if (!db.objectStoreNames.contains(SETTINGS_STORE)) {
                    const settingsStore = db.createObjectStore(SETTINGS_STORE, { keyPath: 'key' });
                    console.log('✅ 通用设置对象存储创建成功');
                }
            };
        });

        return initPromise;
    }

    /**
     * 从 localStorage 迁移数据到 IndexedDB
     * @returns {Promise<boolean>}
     */
    async function migrateFromLocalStorage() {
        try {
            console.log('🔄 检查是否需要从 localStorage 迁移数据...');
            
            const legacyData = localStorage.getItem(LEGACY_STORAGE_KEY);
            if (!legacyData) {
                console.log('ℹ️ 没有发现 localStorage 中的旧数据');
                return false;
            }

            const sessions = JSON.parse(legacyData);
            if (!Array.isArray(sessions) || sessions.length === 0) {
                console.log('ℹ️ localStorage 中没有有效的会话数据');
                return false;
            }

            console.log(`🔄 开始迁移 ${sessions.length} 个会话...`);
            
            const db = await initDB();
            const transaction = db.transaction([STORE_NAME], 'readwrite');
            const store = transaction.objectStore(STORE_NAME);

            let migratedCount = 0;
            for (const session of sessions) {
                try {
                    // 确保日期字段是 ISO 字符串格式
                    if (session.createdAt) {
                        session.createdAt = new Date(session.createdAt).toISOString();
                    }
                    if (session.updatedAt) {
                        session.updatedAt = new Date(session.updatedAt).toISOString();
                    }
                    
                    await new Promise((resolve, reject) => {
                        const request = store.put(session);
                        request.onsuccess = () => resolve();
                        request.onerror = () => reject(request.error);
                    });
                    
                    migratedCount++;
                } catch (error) {
                    console.error(`迁移会话失败 ${session.sessionId}:`, error);
                }
            }

            await new Promise((resolve, reject) => {
                transaction.oncomplete = () => resolve();
                transaction.onerror = () => reject(transaction.error);
            });

            console.log(`✅ 成功迁移 ${migratedCount} 个会话`);
            
            // 迁移成功后，删除 localStorage 中的旧数据
            localStorage.removeItem(LEGACY_STORAGE_KEY);
            console.log('✅ 已清理 localStorage 中的旧数据');
            
            return true;
        } catch (error) {
            console.error('❌ 数据迁移失败:', error);
            return false;
        }
    }

    /**
     * 保存单个会话
     * @param {Object} session - 会话对象
     * @returns {Promise<boolean>}
     */
    async function saveSession(session) {
        try {
            if (!session || !session.sessionId) {
                console.error('❌ 保存会话失败: 无效的会话数据');
                return false;
            }

            const db = await initDB();
            
            // 确保日期字段是 ISO 字符串格式
            const sessionToSave = { ...session };
            if (sessionToSave.createdAt) {
                sessionToSave.createdAt = new Date(sessionToSave.createdAt).toISOString();
            }
            if (sessionToSave.updatedAt) {
                sessionToSave.updatedAt = new Date(sessionToSave.updatedAt).toISOString();
            }
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.put(sessionToSave);

                request.onsuccess = () => {
                    console.log(`✅ 会话已保存: ${session.sessionId}`);
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存会话失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存会话失败:', error);
            return false;
        }
    }

    /**
     * 批量保存会话
     * @param {Array} sessions - 会话数组
     * @returns {Promise<boolean>}
     */
    async function saveSessions(sessions) {
        try {
            if (!sessions || !Array.isArray(sessions)) {
                console.error('❌ 保存会话失败: 无效的会话数据');
                return false;
            }

            const db = await initDB();
            const transaction = db.transaction([STORE_NAME], 'readwrite');
            const store = transaction.objectStore(STORE_NAME);

            for (const session of sessions) {
                if (!session || !session.sessionId) {
                    continue;
                }
                
                // 确保日期字段是 ISO 字符串格式
                const sessionToSave = { ...session };
                if (sessionToSave.createdAt) {
                    sessionToSave.createdAt = new Date(sessionToSave.createdAt).toISOString();
                }
                if (sessionToSave.updatedAt) {
                    sessionToSave.updatedAt = new Date(sessionToSave.updatedAt).toISOString();
                }
                
                store.put(sessionToSave);
            }

            return new Promise((resolve, reject) => {
                transaction.oncomplete = () => {
                    console.log(`✅ 成功保存 ${sessions.length} 个会话`);
                    resolve(true);
                };

                transaction.onerror = () => {
                    console.error('❌ 批量保存会话失败:', transaction.error);
                    reject(transaction.error);
                };
            });
        } catch (error) {
            console.error('❌ 批量保存会话失败:', error);
            return false;
        }
    }

    /**
     * 加载所有会话
     * @returns {Promise<Array>}
     */
    async function loadSessions() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readonly');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.getAll();

                request.onsuccess = () => {
                    const sessions = request.result || [];
                    console.log(`✅ 加载了 ${sessions.length} 个会话`);
                    resolve(sessions);
                };

                request.onerror = () => {
                    console.error('❌ 加载会话失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 加载会话失败:', error);
            return [];
        }
    }

    /**
     * 获取单个会话
     * @param {string} sessionId - 会话ID
     * @returns {Promise<Object|null>}
     */
    async function getSession(sessionId) {
        try {
            if (!sessionId) {
                console.error('❌ 获取会话失败: 会话ID为空');
                return null;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readonly');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.get(sessionId);

                request.onsuccess = () => {
                    const session = request.result || null;
                    if (session) {
                        console.log(`✅ 获取会话成功: ${sessionId}`);
                    } else {
                        console.log(`ℹ️ 会话不存在: ${sessionId}`);
                    }
                    resolve(session);
                };

                request.onerror = () => {
                    console.error('❌ 获取会话失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取会话失败:', error);
            return null;
        }
    }

    /**
     * 删除会话
     * @param {string} sessionId - 会话ID
     * @returns {Promise<boolean>}
     */
    async function deleteSession(sessionId) {
        try {
            if (!sessionId) {
                console.error('❌ 删除会话失败: 会话ID为空');
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.delete(sessionId);

                request.onsuccess = () => {
                    console.log(`✅ 会话已删除: ${sessionId}`);
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 删除会话失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 删除会话失败:', error);
            return false;
        }
    }

    /**
     * 保存输出结果区域状态
     * @param {Object} state - OutputPanelState
     * @returns {Promise<boolean>}
     */
    async function saveSessionOutput(state) {
        try {
            if (!state || !state.sessionId) {
                console.error('❌ 保存输出状态失败: 无效的会话数据');
                return false;
            }

            const db = await initDB();

            const stateToSave = { ...state };
            if (stateToSave.updatedAt) {
                stateToSave.updatedAt = new Date(stateToSave.updatedAt).toISOString();
            } else {
                stateToSave.updatedAt = new Date().toISOString();
            }

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([OUTPUT_STORE], 'readwrite');
                const store = transaction.objectStore(OUTPUT_STORE);
                const request = store.put(stateToSave);

                request.onsuccess = () => {
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存输出状态失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存输出状态失败:', error);
            return false;
        }
    }

    /**
     * 获取输出结果区域状态
     * @param {string} sessionId
     * @returns {Promise<Object|null>}
     */
    async function getSessionOutput(sessionId) {
        try {
            if (!sessionId) {
                return null;
            }

            const db = await initDB();

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([OUTPUT_STORE], 'readonly');
                const store = transaction.objectStore(OUTPUT_STORE);
                const request = store.get(sessionId);

                request.onsuccess = () => {
                    resolve(request.result || null);
                };

                request.onerror = () => {
                    console.error('❌ 获取输出状态失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取输出状态失败:', error);
            return null;
        }
    }

    /**
     * 删除输出结果区域状态
     * @param {string} sessionId
     * @returns {Promise<boolean>}
     */
    async function deleteSessionOutput(sessionId) {
        try {
            if (!sessionId) {
                return false;
            }

            const db = await initDB();

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([OUTPUT_STORE], 'readwrite');
                const store = transaction.objectStore(OUTPUT_STORE);
                const request = store.delete(sessionId);

                request.onsuccess = () => resolve(true);
                request.onerror = () => {
                    console.error('❌ 删除输出状态失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 删除输出状态失败:', error);
            return false;
        }
    }

    /**
     * 清空所有会话
     * @returns {Promise<boolean>}
     */
    async function clearAllSessions() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readwrite');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.clear();

                request.onsuccess = () => {
                    console.log('✅ 所有会话已清空');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 清空会话失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 清空会话失败:', error);
            return false;
        }
    }

    /**
     * 获取会话数量
     * @returns {Promise<number>}
     */
    async function getSessionCount() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([STORE_NAME], 'readonly');
                const store = transaction.objectStore(STORE_NAME);
                const request = store.count();

                request.onsuccess = () => {
                    const count = request.result || 0;
                    console.log(`ℹ️ 当前会话数量: ${count}`);
                    resolve(count);
                };

                request.onerror = () => {
                    console.error('❌ 获取会话数量失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取会话数量失败:', error);
            return 0;
        }
    }

    /**
     * 获取数据库存储大小估算（字节）
     * @returns {Promise<number>}
     */
    async function getStorageSize() {
        try {
            if ('storage' in navigator && 'estimate' in navigator.storage) {
                const estimate = await navigator.storage.estimate();
                const usage = estimate.usage || 0;
                const quota = estimate.quota || 0;
                
                console.log(`ℹ️ 存储使用情况: ${(usage / 1024 / 1024).toFixed(2)} MB / ${(quota / 1024 / 1024).toFixed(2)} MB`);
                
                return {
                    usage: usage,
                    quota: quota,
                    usagePercent: quota > 0 ? (usage / quota * 100).toFixed(2) : 0
                };
            } else {
                console.warn('⚠️ 浏览器不支持存储估算 API');
                return { usage: 0, quota: 0, usagePercent: 0 };
            }
        } catch (error) {
            console.error('❌ 获取存储大小失败:', error);
            return { usage: 0, quota: 0, usagePercent: 0 };
        }
    }

    // ==================== 模板管理 ====================

    /**
     * 保存模板
     * @param {Object} template - 模板对象
     * @returns {Promise<boolean>}
     */
    async function saveTemplate(template) {
        try {
            if (!template || !template.id) {
                console.error('❌ 保存模板失败: 无效的模板数据');
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readwrite');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const request = store.put(template);

                request.onsuccess = () => {
                    console.log(`✅ 模板已保存: ${template.id}`);
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存模板失败:', error);
            return false;
        }
    }

    /**
     * 获取单个模板
     * @param {string} templateId - 模板ID
     * @returns {Promise<Object|null>}
     */
    async function getTemplate(templateId) {
        try {
            if (!templateId) {
                console.error('❌ 获取模板失败: 模板ID为空');
                return null;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readonly');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const request = store.get(templateId);

                request.onsuccess = () => {
                    resolve(request.result || null);
                };

                request.onerror = () => {
                    console.error('❌ 获取模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取模板失败:', error);
            return null;
        }
    }

    /**
     * 获取所有模板
     * @returns {Promise<Array>}
     */
    async function getAllTemplates() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readonly');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const request = store.getAll();

                request.onsuccess = () => {
                    const templates = request.result || [];
                    console.log(`✅ 加载了 ${templates.length} 个模板`);
                    resolve(templates);
                };

                request.onerror = () => {
                    console.error('❌ 加载模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 加载模板失败:', error);
            return [];
        }
    }

    /**
     * 按分类获取模板
     * @param {string} category - 分类名称
     * @returns {Promise<Array>}
     */
    async function getTemplatesByCategory(category) {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readonly');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const index = store.index('category');
                const request = index.getAll(category);

                request.onsuccess = () => {
                    resolve(request.result || []);
                };

                request.onerror = () => {
                    console.error('❌ 获取分类模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取分类模板失败:', error);
            return [];
        }
    }

    /**
     * 删除模板
     * @param {string} templateId - 模板ID
     * @returns {Promise<boolean>}
     */
    async function deleteTemplate(templateId) {
        try {
            if (!templateId) {
                console.error('❌ 删除模板失败: 模板ID为空');
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readwrite');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const request = store.delete(templateId);

                request.onsuccess = () => {
                    console.log(`✅ 模板已删除: ${templateId}`);
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 删除模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 删除模板失败:', error);
            return false;
        }
    }

    /**
     * 清空所有模板
     * @returns {Promise<boolean>}
     */
    async function clearAllTemplates() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([TEMPLATE_STORE], 'readwrite');
                const store = transaction.objectStore(TEMPLATE_STORE);
                const request = store.clear();

                request.onsuccess = () => {
                    console.log('✅ 所有模板已清空');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 清空模板失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 清空模板失败:', error);
            return false;
        }
    }

    /**
     * 初始化默认模板
     * @returns {Promise<boolean>}
     */
    async function initDefaultTemplates() {
        try {
            const existingTemplates = await getAllTemplates();
            if (existingTemplates.length > 0) {
                console.log('ℹ️ 已存在模板，跳过初始化默认模板');
                return true;
            }

            console.log('🔄 开始初始化默认模板...');

            const defaultTemplates = [
                {
                    id: 'optimize-code',
                    title: '优化代码',
                    content: '请优化以下代码的性能和可读性，并说明优化的原因：\n\n{{code}}',
                    category: 'optimization',
                    icon: '🔧',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                },
                {
                    id: 'add-comments',
                    title: '添加注释',
                    content: '请为以下代码添加详细的中文注释，包括函数说明、参数说明和关键逻辑说明：\n\n{{code}}',
                    category: 'documentation',
                    icon: '📝',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                },
                {
                    id: 'fix-bug',
                    title: '修复 Bug',
                    content: '请帮我分析并修复以下代码中的 Bug，并解释问题原因：\n\n{{code}}\n\n错误信息：{{error}}',
                    category: 'debugging',
                    icon: '🐛',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code', 'error'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                },
                {
                    id: 'refactor-code',
                    title: '重构代码',
                    content: '请重构以下代码，提高代码质量和可维护性，遵循 SOLID 原则：\n\n{{code}}',
                    category: 'refactoring',
                    icon: '🔄',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                },
                {
                    id: 'generate-tests',
                    title: '生成测试',
                    content: '请为以下代码生成单元测试用例，使用 {{framework}} 测试框架：\n\n{{code}}',
                    category: 'testing',
                    icon: '🧪',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code', 'framework'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                },
                {
                    id: 'code-review',
                    title: '代码审查',
                    content: '请进行代码审查，指出潜在问题和改进建议，包括：\n1. 代码质量\n2. 安全性\n3. 性能\n4. 可维护性\n\n{{code}}',
                    category: 'review',
                    icon: '👁️',
                    isCustom: false,
                    isFavorite: false,
                    variables: ['code'],
                    createdAt: new Date().toISOString(),
                    updatedAt: new Date().toISOString()
                }
            ];

            const db = await initDB();
            const transaction = db.transaction([TEMPLATE_STORE], 'readwrite');
            const store = transaction.objectStore(TEMPLATE_STORE);

            for (const template of defaultTemplates) {
                store.put(template);
            }

            return new Promise((resolve, reject) => {
                transaction.oncomplete = () => {
                    console.log(`✅ 成功初始化 ${defaultTemplates.length} 个默认模板`);
                    resolve(true);
                };

                transaction.onerror = () => {
                    console.error('❌ 初始化默认模板失败:', transaction.error);
                    reject(transaction.error);
                };
            });
        } catch (error) {
            console.error('❌ 初始化默认模板失败:', error);
            return false;
        }
    }

    // ==================== 输入历史管理 ====================

    /**
     * 保存输入历史
     * @param {string} text - 输入文本
     * @returns {Promise<boolean>}
     */
    async function saveInputHistory(text) {
        try {
            if (!text || text.trim().length === 0) {
                return false;
            }

            const db = await initDB();
            
            const history = {
                text: text.trim(),
                timestamp: new Date().toISOString()
            };

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([HISTORY_STORE], 'readwrite');
                const store = transaction.objectStore(HISTORY_STORE);
                const request = store.add(history);

                request.onsuccess = () => {
                    console.log('✅ 输入历史已保存');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存输入历史失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存输入历史失败:', error);
            return false;
        }
    }

    /**
     * 获取最近的输入历史
     * @param {number} limit - 限制数量，默认 50
     * @returns {Promise<Array>}
     */
    async function getRecentInputHistory(limit = 50) {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([HISTORY_STORE], 'readonly');
                const store = transaction.objectStore(HISTORY_STORE);
                const index = store.index('timestamp');
                const request = index.openCursor(null, 'prev'); // 降序

                const results = [];
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor && results.length < limit) {
                        results.push(cursor.value);
                        cursor.continue();
                    } else {
                        console.log(`✅ 加载了 ${results.length} 条输入历史`);
                        resolve(results);
                    }
                };

                request.onerror = () => {
                    console.error('❌ 获取输入历史失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取输入历史失败:', error);
            return [];
        }
    }

    /**
     * 搜索输入历史
     * @param {string} searchText - 搜索文本
     * @param {number} limit - 限制数量，默认 10
     * @returns {Promise<Array>}
     */
    async function searchInputHistory(searchText, limit = 10) {
        try {
            if (!searchText || searchText.trim().length === 0) {
                return [];
            }

            const allHistory = await getRecentInputHistory(200); // 获取最近 200 条
            const searchLower = searchText.toLowerCase();
            
            const filtered = allHistory
                .filter(item => item.text.toLowerCase().includes(searchLower))
                .slice(0, limit);

            console.log(`✅ 搜索到 ${filtered.length} 条匹配的历史记录`);
            return filtered;
        } catch (error) {
            console.error('❌ 搜索输入历史失败:', error);
            return [];
        }
    }

    /**
     * 清空输入历史
     * @returns {Promise<boolean>}
     */
    async function clearInputHistory() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([HISTORY_STORE], 'readwrite');
                const store = transaction.objectStore(HISTORY_STORE);
                const request = store.clear();

                request.onsuccess = () => {
                    console.log('✅ 输入历史已清空');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 清空输入历史失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 清空输入历史失败:', error);
            return false;
        }
    }

    // ==================== 性能指标管理 ====================

    /**
     * 记录性能指标
     * @param {string} operation - 操作名称
     * @param {number} sessionCount - 会话数量
     * @param {number} duration - 耗时（毫秒）
     * @returns {Promise<boolean>}
     */
    async function recordPerformanceMetric(operation, sessionCount, duration) {
        try {
            const db = await initDB();
            
            const metric = {
                operation: operation,
                sessionCount: sessionCount,
                duration: duration,
                timestamp: Date.now()
            };

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([METRICS_STORE], 'readwrite');
                const store = transaction.objectStore(METRICS_STORE);
                const request = store.add(metric);

                request.onsuccess = () => {
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 记录性能指标失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 记录性能指标失败:', error);
            return false;
        }
    }

    /**
     * 获取最近的性能指标
     * @param {number} limit - 限制数量，默认 100
     * @returns {Promise<Array>}
     */
    async function getRecentPerformanceMetrics(limit = 100) {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([METRICS_STORE], 'readonly');
                const store = transaction.objectStore(METRICS_STORE);
                const index = store.index('timestamp');
                const request = index.openCursor(null, 'prev'); // 降序

                const results = [];
                
                request.onsuccess = (event) => {
                    const cursor = event.target.result;
                    if (cursor && results.length < limit) {
                        results.push(cursor.value);
                        cursor.continue();
                    } else {
                        resolve(results);
                    }
                };

                request.onerror = () => {
                    console.error('❌ 获取性能指标失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取性能指标失败:', error);
            return [];
        }
    }

    /**
     * 获取性能统计
     * @returns {Promise<Object|null>}
     */
    async function getPerformanceStats() {
        try {
            const metrics = await getRecentPerformanceMetrics(100);
            
            if (metrics.length === 0) {
                return null;
            }
            
            // 按操作类型分组统计
            const stats = {};
            metrics.forEach(metric => {
                if (!stats[metric.operation]) {
                    stats[metric.operation] = {
                        count: 0,
                        totalDuration: 0,
                        avgDuration: 0,
                        minDuration: Infinity,
                        maxDuration: 0
                    };
                }
                
                const stat = stats[metric.operation];
                stat.count++;
                stat.totalDuration += metric.duration;
                stat.minDuration = Math.min(stat.minDuration, metric.duration);
                stat.maxDuration = Math.max(stat.maxDuration, metric.duration);
            });
            
            // 计算平均值
            Object.keys(stats).forEach(operation => {
                const stat = stats[operation];
                stat.avgDuration = stat.totalDuration / stat.count;
            });
            
            return stats;
        } catch (error) {
            console.error('❌ 获取性能统计失败:', error);
            return null;
        }
    }

    /**
     * 清空性能指标
     * @returns {Promise<boolean>}
     */
    async function clearPerformanceMetrics() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([METRICS_STORE], 'readwrite');
                const store = transaction.objectStore(METRICS_STORE);
                const request = store.clear();

                request.onsuccess = () => {
                    console.log('✅ 性能指标已清空');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 清空性能指标失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 清空性能指标失败:', error);
            return false;
        }
    }

    // ==================== 快捷操作管理 ====================

    /**
     * 保存快捷操作
     * @param {Object} action - 快捷操作对象
     * @returns {Promise<boolean>}
     */
    async function saveQuickAction(action) {
        try {
            if (!action || !action.id) {
                console.error('❌ 保存快捷操作失败: 无效的数据');
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([QUICK_ACTIONS_STORE], 'readwrite');
                const store = transaction.objectStore(QUICK_ACTIONS_STORE);
                const request = store.put(action);

                request.onsuccess = () => {
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存快捷操作失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存快捷操作失败:', error);
            return false;
        }
    }

    /**
     * 批量保存快捷操作
     * @param {Array} actions - 快捷操作数组
     * @returns {Promise<boolean>}
     */
    async function saveAllQuickActions(actions) {
        try {
            if (!actions || !Array.isArray(actions)) {
                console.error('❌ 保存快捷操作失败: 无效的数据');
                return false;
            }

            const db = await initDB();
            const transaction = db.transaction([QUICK_ACTIONS_STORE], 'readwrite');
            const store = transaction.objectStore(QUICK_ACTIONS_STORE);

            // 先清空现有数据
            store.clear();

            // 保存所有新数据
            for (const action of actions) {
                if (action && action.id) {
                    store.put(action);
                }
            }

            return new Promise((resolve, reject) => {
                transaction.oncomplete = () => {
                    console.log(`✅ 成功保存 ${actions.length} 个快捷操作`);
                    resolve(true);
                };

                transaction.onerror = () => {
                    console.error('❌ 批量保存快捷操作失败:', transaction.error);
                    reject(transaction.error);
                };
            });
        } catch (error) {
            console.error('❌ 批量保存快捷操作失败:', error);
            return false;
        }
    }

    /**
     * 获取所有快捷操作
     * @returns {Promise<Array>}
     */
    async function getAllQuickActions() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([QUICK_ACTIONS_STORE], 'readonly');
                const store = transaction.objectStore(QUICK_ACTIONS_STORE);
                const request = store.getAll();

                request.onsuccess = () => {
                    const actions = request.result || [];
                    // 按 order 排序
                    actions.sort((a, b) => (a.order || 0) - (b.order || 0));
                    resolve(actions);
                };

                request.onerror = () => {
                    console.error('❌ 获取快捷操作失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取快捷操作失败:', error);
            return [];
        }
    }

    /**
     * 删除快捷操作
     * @param {string} actionId - 快捷操作ID
     * @returns {Promise<boolean>}
     */
    async function deleteQuickAction(actionId) {
        try {
            if (!actionId) {
                console.error('❌ 删除快捷操作失败: ID为空');
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([QUICK_ACTIONS_STORE], 'readwrite');
                const store = transaction.objectStore(QUICK_ACTIONS_STORE);
                const request = store.delete(actionId);

                request.onsuccess = () => {
                    console.log(`✅ 快捷操作已删除: ${actionId}`);
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 删除快捷操作失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 删除快捷操作失败:', error);
            return false;
        }
    }

    /**
     * 清空所有快捷操作
     * @returns {Promise<boolean>}
     */
    async function clearAllQuickActions() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([QUICK_ACTIONS_STORE], 'readwrite');
                const store = transaction.objectStore(QUICK_ACTIONS_STORE);
                const request = store.clear();

                request.onsuccess = () => {
                    console.log('✅ 所有快捷操作已清空');
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 清空快捷操作失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 清空快捷操作失败:', error);
            return false;
        }
    }

    // ==================== 通用设置管理 ====================

    /**
     * 保存设置
     * @param {string} key - 设置键
     * @param {any} value - 设置值
     * @returns {Promise<boolean>}
     */
    async function saveSetting(key, value) {
        try {
            if (!key) {
                console.error('❌ 保存设置失败: 键为空');
                return false;
            }

            const db = await initDB();
            
            const setting = {
                key: key,
                value: value,
                updatedAt: new Date().toISOString()
            };

            return new Promise((resolve, reject) => {
                const transaction = db.transaction([SETTINGS_STORE], 'readwrite');
                const store = transaction.objectStore(SETTINGS_STORE);
                const request = store.put(setting);

                request.onsuccess = () => {
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 保存设置失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 保存设置失败:', error);
            return false;
        }
    }

    /**
     * 获取设置
     * @param {string} key - 设置键
     * @param {any} defaultValue - 默认值
     * @returns {Promise<any>}
     */
    async function getSetting(key, defaultValue = null) {
        try {
            if (!key) {
                return defaultValue;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([SETTINGS_STORE], 'readonly');
                const store = transaction.objectStore(SETTINGS_STORE);
                const request = store.get(key);

                request.onsuccess = () => {
                    const result = request.result;
                    if (result && result.value !== undefined) {
                        resolve(result.value);
                    } else {
                        resolve(defaultValue);
                    }
                };

                request.onerror = () => {
                    console.error('❌ 获取设置失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取设置失败:', error);
            return defaultValue;
        }
    }

    /**
     * 删除设置
     * @param {string} key - 设置键
     * @returns {Promise<boolean>}
     */
    async function deleteSetting(key) {
        try {
            if (!key) {
                return false;
            }

            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([SETTINGS_STORE], 'readwrite');
                const store = transaction.objectStore(SETTINGS_STORE);
                const request = store.delete(key);

                request.onsuccess = () => {
                    resolve(true);
                };

                request.onerror = () => {
                    console.error('❌ 删除设置失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 删除设置失败:', error);
            return false;
        }
    }

    /**
     * 获取所有设置
     * @returns {Promise<Object>}
     */
    async function getAllSettings() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([SETTINGS_STORE], 'readonly');
                const store = transaction.objectStore(SETTINGS_STORE);
                const request = store.getAll();

                request.onsuccess = () => {
                    const settings = {};
                    const results = request.result || [];
                    results.forEach(item => {
                        settings[item.key] = item.value;
                    });
                    resolve(settings);
                };

                request.onerror = () => {
                    console.error('❌ 获取所有设置失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取所有设置失败:', error);
            return {};
        }
    }

    // ==================== 数据迁移 ====================

    /**
     * 从 localStorage 迁移所有数据到 IndexedDB
     * @returns {Promise<boolean>}
     */
    async function migrateAllFromLocalStorage() {
        try {
            console.log('🔄 开始从 localStorage 迁移所有数据...');
            
            // 迁移会话数据（已有的函数）
            await migrateFromLocalStorage();
            
            // 迁移性能指标
            const perfMetricsKey = 'session_performance_metrics';
            const perfData = localStorage.getItem(perfMetricsKey);
            if (perfData) {
                try {
                    const metrics = JSON.parse(perfData);
                    if (Array.isArray(metrics)) {
                        console.log(`🔄 迁移 ${metrics.length} 条性能指标...`);
                        for (const metric of metrics) {
                            await recordPerformanceMetric(
                                metric.operation, 
                                metric.sessionCount, 
                                metric.duration
                            );
                        }
                        localStorage.removeItem(perfMetricsKey);
                        console.log('✅ 性能指标迁移完成');
                    }
                } catch (e) {
                    console.error('性能指标迁移失败:', e);
                    // 即使解析失败也删除损坏的数据
                    localStorage.removeItem(perfMetricsKey);
                }
            }
            
            // 迁移快捷操作
            const quickActionsKey = 'webcli_quick_actions';
            const actionsData = localStorage.getItem(quickActionsKey);
            if (actionsData) {
                try {
                    const actions = JSON.parse(actionsData);
                    if (Array.isArray(actions) && actions.length > 0) {
                        console.log(`🔄 迁移 ${actions.length} 个快捷操作...`);
                        await saveAllQuickActions(actions);
                        localStorage.removeItem(quickActionsKey);
                        console.log('✅ 快捷操作迁移完成');
                    }
                } catch (e) {
                    console.error('快捷操作迁移失败:', e);
                    localStorage.removeItem(quickActionsKey);
                }
            }
            
            // 迁移语言设置
            const langKey = 'webcli_language';
            const langValue = localStorage.getItem(langKey);
            if (langValue) {
                await saveSetting('language', langValue);
                localStorage.removeItem(langKey);
                console.log('✅ 语言设置迁移完成');
            }
            
            // 清理所有已知的旧 localStorage 数据
            cleanupLocalStorage();
            
            console.log('✅ 所有数据迁移完成');
            return true;
        } catch (error) {
            console.error('❌ 数据迁移失败:', error);
            return false;
        }
    }

    /**
     * 清理所有已知的 localStorage 旧数据
     */
    function cleanupLocalStorage() {
        const keysToRemove = [
            'webcli_sessions',
            'session_performance_metrics',
            'webcli_quick_actions',
            'webcli_language'
        ];
        
        let removedCount = 0;
        for (const key of keysToRemove) {
            if (localStorage.getItem(key) !== null) {
                try {
                    localStorage.removeItem(key);
                    removedCount++;
                    console.log(`🗑️ 已清理 localStorage: ${key}`);
                } catch (e) {
                    console.error(`清理 ${key} 失败:`, e);
                }
            }
        }
        
        if (removedCount > 0) {
            console.log(`✅ 已清理 ${removedCount} 个旧的 localStorage 项`);
        }
    }

    /**
     * 获取 localStorage 使用情况
     * @returns {Object}
     */
    function getLocalStorageInfo() {
        try {
            let total = 0;
            const items = {};
            
            for (let key in localStorage) {
                if (localStorage.hasOwnProperty(key)) {
                    const value = localStorage[key];
                    const size = (key.length + value.length) * 2;
                    total += size;
                    items[key] = {
                        size: size,
                        sizeMB: (size / (1024 * 1024)).toFixed(4)
                    };
                }
            }
            
            return {
                totalBytes: total,
                totalMB: (total / (1024 * 1024)).toFixed(2),
                itemCount: Object.keys(items).length,
                items: items
            };
        } catch (e) {
            console.error('获取 localStorage 信息失败:', e);
            return { totalBytes: 0, totalMB: '0.00', itemCount: 0, items: {} };
        }
    }

    // ==================== IndexedDB 迁移到 SQLite 后端 ====================

    /**
     * 迁移所有数据到后端 SQLite
     * @returns {Promise<Object>} 迁移结果
     */
    async function migrateToBackend() {
        console.log('🔄 开始迁移 IndexedDB 数据到后端...');
        const results = {
            sessions: { success: false, count: 0 },
            templates: { success: false, count: 0 },
            outputs: { success: false, count: 0 },
            inputHistory: { success: false, count: 0 },
            quickActions: { success: false, count: 0 },
            settings: { success: false, count: 0 }
        };

        try {
            // 迁移会话
            const sessions = await loadSessions();
            if (sessions && sessions.length > 0) {
                const response = await fetch('/api/migration/sessions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(sessions)
                });
                const data = await response.json();
                results.sessions = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 会话迁移: ${data.migratedCount || 0} 个`);
            }

            // 迁移模板
            const templates = await getAllTemplates();
            if (templates && templates.length > 0) {
                const response = await fetch('/api/migration/templates', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(templates)
                });
                const data = await response.json();
                results.templates = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 模板迁移: ${data.migratedCount || 0} 个`);
            }

            // 迁移输出状态
            const outputs = await getAllSessionOutputs();
            if (outputs && outputs.length > 0) {
                const response = await fetch('/api/migration/session-outputs', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(outputs)
                });
                const data = await response.json();
                results.outputs = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 输出状态迁移: ${data.migratedCount || 0} 个`);
            }

            // 迁移输入历史
            const inputHistory = await getRecentInputHistory(1000);
            if (inputHistory && inputHistory.length > 0) {
                const response = await fetch('/api/migration/input-history', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(inputHistory)
                });
                const data = await response.json();
                results.inputHistory = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 输入历史迁移: ${data.migratedCount || 0} 条`);
            }

            // 迁移快捷操作
            const quickActions = await getAllQuickActions();
            if (quickActions && quickActions.length > 0) {
                const response = await fetch('/api/migration/quick-actions', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(quickActions)
                });
                const data = await response.json();
                results.quickActions = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 快捷操作迁移: ${data.migratedCount || 0} 个`);
            }

            // 迁移设置
            const settings = await getAllSettings();
            if (settings && Object.keys(settings).length > 0) {
                const response = await fetch('/api/migration/settings', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(settings)
                });
                const data = await response.json();
                results.settings = { success: data.success, count: data.migratedCount || 0 };
                console.log(`✅ 设置迁移: ${data.migratedCount || 0} 个`);
            }

            console.log('✅ IndexedDB 数据迁移完成', results);
            return { success: true, results };
        } catch (error) {
            console.error('❌ 迁移到后端失败:', error);
            return { success: false, error: error.message, results };
        }
    }

    /**
     * 获取所有会话输出状态（用于迁移）
     * @returns {Promise<Array>}
     */
    async function getAllSessionOutputs() {
        try {
            const db = await initDB();
            
            return new Promise((resolve, reject) => {
                const transaction = db.transaction([OUTPUT_STORE], 'readonly');
                const store = transaction.objectStore(OUTPUT_STORE);
                const request = store.getAll();

                request.onsuccess = () => {
                    const outputs = request.result || [];
                    resolve(outputs.map(o => ({
                        sessionId: o.sessionId,
                        rawOutput: o.rawOutput || '',
                        eventsJson: o.eventsJson || JSON.stringify(o.jsonlEvents || []),
                        displayedEventCount: o.displayedEventCount || 20
                    })));
                };

                request.onerror = () => {
                    console.error('❌ 获取所有输出状态失败:', request.error);
                    reject(request.error);
                };
            });
        } catch (error) {
            console.error('❌ 获取所有输出状态失败:', error);
            return [];
        }
    }

    /**
     * 清理 IndexedDB 所有数据（迁移完成后调用）
     * @returns {Promise<boolean>}
     */
    async function clearAllIndexedDBData() {
        try {
            console.log('🗑️ 开始清理 IndexedDB 数据...');
            
            await clearAllSessions();
            await clearAllTemplates();
            await clearInputHistory();
            await clearAllQuickActions();
            
            // 清理输出状态
            const db = await initDB();
            await new Promise((resolve, reject) => {
                const transaction = db.transaction([OUTPUT_STORE], 'readwrite');
                const store = transaction.objectStore(OUTPUT_STORE);
                const request = store.clear();
                request.onsuccess = () => resolve(true);
                request.onerror = () => reject(request.error);
            });
            
            // 清理设置
            await new Promise((resolve, reject) => {
                const transaction = db.transaction([SETTINGS_STORE], 'readwrite');
                const store = transaction.objectStore(SETTINGS_STORE);
                const request = store.clear();
                request.onsuccess = () => resolve(true);
                request.onerror = () => reject(request.error);
            });
            
            console.log('✅ IndexedDB 数据已清理');
            return true;
        } catch (error) {
            console.error('❌ 清理 IndexedDB 失败:', error);
            return false;
        }
    }

    /**
     * 检查迁移状态
     * @returns {Promise<Object>}
     */
    async function checkMigrationStatus() {
        try {
            const response = await fetch('/api/migration/status');
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('❌ 检查迁移状态失败:', error);
            return { success: false, error: error.message };
        }
    }

    /**
     * 检查是否需要迁移
     * @returns {Promise<boolean>}
     */
    async function needsMigration() {
        try {
            const sessions = await loadSessions();
            const templates = await getAllTemplates();
            const hasLocalData = (sessions && sessions.length > 0) || 
                                 (templates && templates.length > 0);
            
            if (!hasLocalData) {
                return false;
            }

            const status = await checkMigrationStatus();
            if (!status.success) {
                return hasLocalData;
            }

            // 如果后端没有数据但本地有，需要迁移
            const counts = status.counts || {};
            const hasBackendData = (counts.sessions || 0) > 0;
            
            return hasLocalData && !hasBackendData;
        } catch (error) {
            console.error('❌ 检查是否需要迁移失败:', error);
            return false;
        }
    }

    // 初始化时自动执行数据迁移和默认模板初始化
    (async function autoInit() {
        try {
            await initDB();
            await migrateAllFromLocalStorage();
            await initDefaultTemplates();
            window.webCliIndexedDBReady = true;
            console.log('✅ IndexedDB 已准备就绪');
            
            // 检查是否需要迁移到后端
            const shouldMigrate = await needsMigration();
            if (shouldMigrate) {
                console.log('📤 检测到需要迁移数据到后端...');
                const result = await migrateToBackend();
                if (result.success) {
                    console.log('✅ 数据已迁移到后端，本地数据已保留');
                }
            }
        } catch (error) {
            console.error('❌ 自动初始化失败:', error);
            window.webCliIndexedDBReady = false;
        }
    })();

    // 导出公共 API
    return {
        // 会话管理
        initDB,
        migrateFromLocalStorage,
        migrateAllFromLocalStorage,
        saveSession,
        saveSessions,
        loadSessions,
        getSession,
        deleteSession,
        clearAllSessions,
        getSessionCount,
        getStorageSize,

        // 输出结果（Tab=输出结果）持久化
        saveSessionOutput,
        getSessionOutput,
        deleteSessionOutput,
        
        // 模板管理
        saveTemplate,
        getTemplate,
        getAllTemplates,
        getTemplatesByCategory,
        deleteTemplate,
        clearAllTemplates,
        initDefaultTemplates,
        
        // 输入历史管理
        saveInputHistory,
        getRecentInputHistory,
        searchInputHistory,
        clearInputHistory,
        
        // 性能指标管理
        recordPerformanceMetric,
        getRecentPerformanceMetrics,
        getPerformanceStats,
        clearPerformanceMetrics,
        
        // 快捷操作管理
        saveQuickAction,
        saveAllQuickActions,
        getAllQuickActions,
        deleteQuickAction,
        clearAllQuickActions,
        
        // 通用设置管理
        saveSetting,
        getSetting,
        deleteSetting,
        getAllSettings,
        
        // 工具函数
        cleanupLocalStorage,
        getLocalStorageInfo,
        
        // 迁移到后端 SQLite
        migrateToBackend,
        getAllSessionOutputs,
        clearAllIndexedDBData,
        checkMigrationStatus,
        needsMigration,
        
        // 检查是否准备就绪
        isReady: () => window.webCliIndexedDBReady === true
    };
})();

console.log('✅ indexeddb-storage.js 加载完成');


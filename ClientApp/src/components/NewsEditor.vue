<template>
  <div class="news-editor">
    <!-- 頁面標題 -->
    <div class="page-header">
      <div class="header-left">
        <router-link to="/dashboard/news" class="back-link">
          ← 返回最新消息列表
        </router-link>
        <h2 class="page-title">{{ isEditing ? '編輯消息' : '發布新消息' }}</h2>
      </div>
      <div class="header-actions">
        <button @click="saveDraft" class="save-draft-btn" :disabled="isSaving">
          💾 {{ isSaving ? '儲存中...' : '儲存草稿' }}
        </button>
        <button @click="publishNews" class="publish-btn" :disabled="isSaving">
          📢 {{ isSaving ? '發布中...' : '立即發布' }}
        </button>
      </div>
    </div>

    <!-- 編輯表單 -->
    <div class="editor-container">
      <div class="editor-main">
        <!-- 消息標題 -->
        <div class="form-group">
          <label class="form-label">消息標題 *</label>
          <input
              v-model="news.title"
              type="text"
              placeholder="輸入消息標題..."
              class="title-input"
              maxlength="200"
          />
          <div class="char-count">{{ news.title.length }}/200</div>
        </div>

        <!-- 消息摘要 -->
        <div class="form-group">
          <label class="form-label">消息摘要</label>
          <textarea
              v-model="news.excerpt"
              placeholder="輸入消息摘要，將顯示在列表和預覽中..."
              class="excerpt-input"
              rows="3"
              maxlength="300"
          ></textarea>
          <div class="char-count">{{ news.excerpt.length }}/300</div>
        </div>

        <!-- 消息圖片 -->
        <div class="form-group">
          <label class="form-label">消息圖片</label>
          <div class="image-upload">
            <div v-if="news.featuredImageUrl" class="image-preview">
              <img :src="news.featuredImageUrl" alt="消息圖片預覽" />
              <button @click="removeImage" class="remove-image-btn">✕</button>
            </div>
            <div v-else class="image-placeholder">
              <input
                  ref="imageInput"
                  type="file"
                  accept="image/*"
                  @change="handleImageUpload"
                  class="image-input"
              />
              <div class="upload-area" @click="$refs.imageInput.click()">
                <div class="upload-icon">📷</div>
                <p>點擊上傳圖片或輸入圖片網址</p>
              </div>
            </div>
            <input
                v-model="news.featuredImageUrl"
                type="url"
                placeholder="或輸入圖片網址..."
                class="image-url-input"
            />
          </div>
        </div>

        <!-- 內容編輯器 -->
        <div class="form-group">
          <label class="form-label">消息內容 *</label>
          <div class="editor-wrapper">
            <div class="editor-toolbar">
              <div class="toolbar-group">
                <button @click="execCommand('bold')" class="toolbar-btn" title="粗體">
                  <strong>B</strong>
                </button>
                <button @click="execCommand('italic')" class="toolbar-btn" title="斜體">
                  <em>I</em>
                </button>
                <button @click="execCommand('underline')" class="toolbar-btn" title="底線">
                  <u>U</u>
                </button>
              </div>

              <div class="toolbar-group">
                <select @change="execCommand('formatBlock', $event.target.value)" class="format-select">
                  <option value="">格式</option>
                  <option value="h2">標題 2</option>
                  <option value="h3">標題 3</option>
                  <option value="p">段落</option>
                </select>
              </div>

              <div class="toolbar-group">
                <button @click="execCommand('insertUnorderedList')" class="toolbar-btn" title="項目符號">
                  •
                </button>
                <button @click="execCommand('insertOrderedList')" class="toolbar-btn" title="編號">
                  1.
                </button>
              </div>

              <div class="toolbar-group">
                <button @click="insertLink" class="toolbar-btn" title="插入連結">
                  🔗
                </button>
                <button @click="insertImage" class="toolbar-btn" title="插入圖片">
                  🖼️
                </button>
              </div>

              <div class="toolbar-group">
                <button @click="toggleHtmlMode" class="toolbar-btn" :class="{ active: showHtml }" title="HTML 模式">
                  &lt;/&gt;
                </button>
              </div>
            </div>

            <div v-if="!showHtml" class="editor-content">
              <div
                  ref="editor"
                  class="rich-editor"
                  contenteditable="true"
                  @input="updateContent"
                  @paste="handlePaste"
                  v-html="news.content"
              ></div>
            </div>

            <div v-else class="html-editor">
              <textarea
                  v-model="news.content"
                  class="html-textarea"
                  placeholder="輸入 HTML 內容..."
              ></textarea>
            </div>
          </div>
        </div>
      </div>

      <!-- 側邊欄設定 -->
      <div class="editor-sidebar">
        <div class="sidebar-section">
          <h3 class="sidebar-title">發布設定</h3>

          <div class="form-group">
            <label class="checkbox-label">
              <input v-model="news.isPublished" type="checkbox" />
              指定時間發布
            </label>
          </div>



          <div v-if="news.isPublished" class="form-group">
            <label class="form-label">發布時間</label>
            <input
                v-model="publishDateTime"
                type="datetime-local"
                class="datetime-input"
            />
          </div>
        </div>

        <div class="sidebar-section">
          <h3 class="sidebar-title">消息統計</h3>
          <div class="stats-info">
            <div class="stat-item">
              <span class="stat-label">字數統計</span>
              <span class="stat-value">{{ wordCount }}</span>
            </div>
            <div class="stat-item" v-if="isEditing && news.publishedAt">
              <span class="stat-label">發布時間</span>
              <span class="stat-value">{{ formatDate(news.publishedAt) }}</span>
            </div>
          </div>
        </div>

        <div class="sidebar-section" v-if="isEditing">
          <h3 class="sidebar-title">消息操作</h3>
          <div class="action-buttons">
            <button @click="previewNews" class="preview-btn">
              👁️ 預覽消息
            </button>
            <button @click="duplicateNews" class="duplicate-btn">
              📋 複製消息
            </button>
            <button @click="deleteNews" class="delete-btn" :disabled="isDeleting">
              🗑️ {{ isDeleting ? '刪除中...' : '刪除消息' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { apiGet, apiPost, apiDelete } from '../utils/api.js'

const route = useRoute()
const router = useRouter()

const isEditing = computed(() => route.params.id !== undefined)
const isSaving = ref(false)
const isDeleting = ref(false)
const showHtml = ref(false)

const news = ref({
  title: '',
  content: '',
  excerpt: '',
  featuredImageUrl: '',
  isPublished: false,
  publishedAt: null
})

const publishDateTime = ref('')
const editor = ref(null)
const imageInput = ref(null)

onMounted(async () => {
  if (isEditing.value) {
    await loadNews()
  } else {
    // 設定預設發布時間為現在
    const now = new Date()
    publishDateTime.value = now.toISOString().slice(0, 16)
  }
})

const loadNews = async () => {
  try {
    // const data = await apiGet(`/api/News/${route.params.id}`)
    // news.value = data

    // 模擬資料
    news.value = {
      title: '2024年度獎學金申請開始！',
      content: '<h2>申請詳情</h2><p>多項獎學金開放申請，包含政府獎學金、學校獎學金等...</p>',
      excerpt: '多項獎學金開放申請，包含政府獎學金、學校獎學金等，申請截止日期為2024年12月31日。',
      featuredImageUrl: 'https://via.placeholder.com/600x400',
      isPublished: true,
      publishedAt: new Date().toISOString()
    }

    if (news.value.publishedAt) {
      publishDateTime.value = new Date(news.value.publishedAt).toISOString().slice(0, 16)
    }
  } catch (error) {
    console.error('Failed to load news:', error)
    alert('載入消息失敗')
    router.push('/dashboard/news')
  }
}

const wordCount = computed(() => {
  const text = news.value.content.replace(/<[^>]*>/g, '')
  return text.length
})

const updateContent = () => {
  if (editor.value) {
    news.value.content = editor.value.innerHTML
  }
}

const execCommand = (command, value = null) => {
  document.execCommand(command, false, value)
  updateContent()
}

const insertLink = () => {
  const url = prompt('請輸入連結網址:')
  if (url) {
    execCommand('createLink', url)
  }
}

const insertImage = () => {
  const url = prompt('請輸入圖片網址:')
  if (url) {
    execCommand('insertImage', url)
  }
}

const toggleHtmlMode = () => {
  showHtml.value = !showHtml.value
}

const handlePaste = (event) => {
  event.preventDefault()
  const text = event.clipboardData.getData('text/plain')
  document.execCommand('insertText', false, text)
}

const handleImageUpload = (event) => {
  const file = event.target.files[0]
  if (file) {
    // 這裡應該上傳圖片到伺服器
    const reader = new FileReader()
    reader.onload = (e) => {
      news.value.featuredImageUrl = e.target.result
    }
    reader.readAsDataURL(file)
  }
}

const removeImage = () => {
  news.value.featuredImageUrl = ''
}

const saveDraft = async () => {
  try {
    isSaving.value = true
    news.value.isPublished = false
    await saveNews()
    alert('草稿已儲存')
  } catch (error) {
    alert('儲存失敗')
  } finally {
    isSaving.value = false
  }
}

const publishNews = async () => {
  if (!news.value.title.trim()) {
    alert('請輸入消息標題')
    return
  }

  if (!news.value.content.trim()) {
    alert('請輸入消息內容')
    return
  }

  try {
    isSaving.value = true
    news.value.isPublished = true

    // 設定發布時間
    if (publishDateTime.value) {
      news.value.publishedAt = new Date(publishDateTime.value).toISOString()
    } else {
      news.value.publishedAt = new Date().toISOString()
    }

    await saveNews()
    alert('消息已發布')
    router.push('/dashboard/news')
  } catch (error) {
    alert('發布失敗')
  } finally {
    isSaving.value = false
  }
}

const saveNews = async () => {
  // 這裡呼叫 API 儲存消息
  console.log('Saving news:', news.value)
  // await apiPost('/api/News', news.value)
}

const previewNews = () => {
  // 開啟預覽視窗
  const previewWindow = window.open('', '_blank')

  const previewContent = `
    <html>
      <head>
        <title>${news.value.title}</title>
        <meta charset="utf-8">
        <style>
          body { 
            font-family: Arial, sans-serif; 
            max-width: 800px; 
            margin: 0 auto; 
            padding: 20px; 
            line-height: 1.6;
          }
          .news-header {
            border-bottom: 2px solid #e2e8f0;
            padding-bottom: 20px;
            margin-bottom: 20px;
          }
          .news-title {
            margin: 0;
            color: #2d3748;
          }
          .important-badge {
            background: #f6ad55;
            color: white;
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 12px;
            margin-left: 10px;
          }
          img { max-width: 100%; height: auto; margin: 20px 0; }
        </style>
      </head>
      <body>
        <div class="news-header">
          <h1 class="news-title">${news.value.title}</h1>
        </div>
        ${news.value.featuredImageUrl ? `<img src="${news.value.featuredImageUrl}" alt="消息圖片">` : ''}
        <div>${news.value.content}</div>
      </body>
    </html>
  `
  previewWindow.document.write(previewContent)
}

const duplicateNews = () => {
  const confirmed = confirm('確定要複製這則消息嗎？')
  if (confirmed) {
    // 複製當前消息並跳轉到新增頁面
    const duplicatedNews = {
      ...news.value,
      title: news.value.title + ' (副本)',
      isPublished: false,
      publishedAt: null
    }

    // 將資料暫存到 sessionStorage
    sessionStorage.setItem('duplicatedNews', JSON.stringify(duplicatedNews))
    router.push('/dashboard/news/create')
  }
}

const deleteNews = async () => {
  const confirmed = confirm(`確定要刪除消息「${news.value.title}」嗎？此操作無法復原。`)
  if (!confirmed) return

  try {
    isDeleting.value = true
    // await apiDelete(`/api/News/${route.params.id}`)
    alert('消息已刪除')
    router.push('/dashboard/news')
  } catch (error) {
    alert('刪除失敗')
  } finally {
    isDeleting.value = false
  }
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 檢查是否有複製的消息資料
onMounted(() => {
  if (!isEditing.value) {
    const duplicated = sessionStorage.getItem('duplicatedNews')
    if (duplicated) {
      news.value = JSON.parse(duplicated)
      sessionStorage.removeItem('duplicatedNews')
    }
  }
})
</script>

<style scoped>
/* 全域設定 box-sizing */
* {
  box-sizing: border-box;
}

.news-editor {
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.back-link {
  color: #3182ce;
  text-decoration: none;
  font-weight: 500;
  font-size: 14px;
  transition: color 0.2s ease;
}

.back-link:hover {
  color: #2c5aa0;
  text-decoration: underline;
}

.page-title {
  margin: 0;
  font-size: 28px;
  font-weight: 700;
  color: #1a202c;
}

.header-actions {
  display: flex;
  gap: 12px;
}

.save-draft-btn, .publish-btn {
  padding: 12px 20px;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  border: 1px solid;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  box-sizing: border-box;
}

.save-draft-btn {
  background: white;
  color: #4a5568;
  border-color: #e2e8f0;
}

.save-draft-btn:hover:not(:disabled) {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.publish-btn {
  background: #38a169;
  color: white;
  border-color: #38a169;
}

.publish-btn:hover:not(:disabled) {
  background: #2f855a;
  transform: translateY(-1px);
}

.save-draft-btn:disabled, .publish-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.editor-container {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 32px;
}

.editor-main {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 24px;
  box-sizing: border-box;
}

.form-group {
  margin-bottom: 24px;
}

.form-label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #4a5568;
  font-size: 14px;
}

/* 修正輸入框超出問題 */
.title-input {
  width: 100%;
  padding: 16px 20px;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 20px;
  font-weight: 600;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.title-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.excerpt-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  resize: vertical;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.excerpt-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.char-count {
  text-align: right;
  font-size: 12px;
  color: #718096;
  margin-top: 4px;
}

.form-hint {
  font-size: 12px;
  color: #718096;
  margin-top: 4px;
}

.image-upload {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.image-preview {
  position: relative;
  max-width: 400px;
}

.image-preview img {
  width: 100%;
  height: auto;
  border-radius: 8px;
}

.remove-image-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  border: none;
  border-radius: 50%;
  width: 28px;
  height: 28px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.2s ease;
}

.remove-image-btn:hover {
  background: rgba(0, 0, 0, 0.9);
}

.image-placeholder {
  position: relative;
}

.image-input {
  display: none;
}

.upload-area {
  border: 2px dashed #cbd5e0;
  border-radius: 8px;
  padding: 40px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.upload-area:hover {
  border-color: #3182ce;
  background: #f7fafc;
}

.upload-icon {
  font-size: 32px;
  margin-bottom: 12px;
}

.upload-area p {
  margin: 0;
  color: #718096;
  font-size: 14px;
}

.image-url-input {
  width: 100%;
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 14px;
  box-sizing: border-box;
}

.image-url-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.editor-wrapper {
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  overflow: hidden;
  transition: border-color 0.2s ease;
  box-sizing: border-box;
}

.editor-wrapper:focus-within {
  border-color: #3182ce;
}

.editor-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  flex-wrap: wrap;
  box-sizing: border-box;
}

.toolbar-group {
  display: flex;
  gap: 4px;
  padding-right: 8px;
  border-right: 1px solid #e2e8f0;
}

.toolbar-group:last-child {
  border-right: none;
}

.toolbar-btn {
  padding: 8px 10px;
  border: 1px solid #e2e8f0;
  background: white;
  border-radius: 6px;
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s ease;
  min-width: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}

.toolbar-btn:hover {
  background: #edf2f7;
  border-color: #cbd5e0;
}

.toolbar-btn.active {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

.format-select {
  padding: 8px 10px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 13px;
  background: white;
  box-sizing: border-box;
}

.rich-editor {
  min-height: 400px;
  padding: 20px;
  outline: none;
  line-height: 1.7;
  font-size: 15px;
  width: 100%;
  box-sizing: border-box;
  overflow-wrap: break-word;
}

.rich-editor:focus {
  background: #fafafa;
}

.html-textarea {
  width: 100%;
  min-height: 400px;
  padding: 20px;
  border: none;
  font-family: 'Courier New', monospace;
  font-size: 14px;
  resize: vertical;
  outline: none;
  box-sizing: border-box;
}

.html-textarea:focus {
  background: #fafafa;
}

.editor-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sidebar-section {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 20px;
  box-sizing: border-box;
}

.sidebar-title {
  margin: 0 0 16px 0;
  font-size: 16px;
  font-weight: 600;
  color: #2d3748;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  font-size: 14px;
  color: #4a5568;
  font-weight: 500;
}

.checkmark {
  width: 16px;
  height: 16px;
  border: 2px solid #e2e8f0;
  border-radius: 4px;
  position: relative;
}

.datetime-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 14px;
  box-sizing: border-box;
}

.datetime-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.stats-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-label {
  font-size: 13px;
  color: #718096;
}

.stat-value {
  font-size: 13px;
  font-weight: 600;
  color: #2d3748;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.preview-btn, .duplicate-btn, .delete-btn {
  width: 100%;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  box-sizing: border-box;
}

.preview-btn {
  background: #edf2f7;
  color: #4a5568;
  border-color: #e2e8f0;
}

.preview-btn:hover {
  background: #e2e8f0;
}

.duplicate-btn {
  background: #e6fffa;
  color: #319795;
  border-color: #81e6d9;
}

.duplicate-btn:hover {
  background: #b2f5ea;
}

.delete-btn {
  background: white;
  color: #e53e3e;
  border-color: #e53e3e;
}

.delete-btn:hover:not(:disabled) {
  background: #e53e3e;
  color: white;
}

.delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 響應式設計 */
@media (max-width: 1024px) {
  .editor-container {
    grid-template-columns: 1fr;
  }

  .page-header {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }

  .header-actions {
    justify-content: stretch;
  }

  .save-draft-btn, .publish-btn {
    flex: 1;
  }
}

@media (max-width: 768px) {
  .news-editor {
    padding: 0 10px;
  }

  .editor-toolbar {
    padding: 8px;
  }

  .toolbar-group {
    padding-right: 4px;
  }

  .rich-editor {
    min-height: 300px;
    padding: 16px;
  }

  .html-textarea {
    min-height: 300px;
    padding: 16px;
  }

  .editor-main {
    padding: 16px;
  }

  .sidebar-section {
    padding: 16px;
  }
}
</style>
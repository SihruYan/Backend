<template>
  <div class="blog-editor">
    <!-- 頁面標題 -->
    <div class="page-header">
      <div class="header-left">
        <router-link to="/dashboard/blog" class="back-link">
          ← 返回部落格列表
        </router-link>
        <h2 class="page-title">{{ isEditing ? '編輯文章' : '新增文章' }}</h2>
      </div>
      <div class="header-actions">
        <button @click="saveDraft" class="save-draft-btn" :disabled="isSaving">
          💾 {{ isSaving ? '儲存中...' : '儲存草稿' }}
        </button>
        <button @click="publishArticle" class="publish-btn" :disabled="isSaving">
          🚀 {{ isSaving ? '發布中...' : '發布文章' }}
        </button>
      </div>
    </div>

    <!-- 編輯表單 -->
    <div class="editor-container">
      <div class="editor-main">
        <!-- 標題 -->
        <div class="form-group">
          <label class="form-label">文章標題 *</label>
          <input
              v-model="article.title"
              type="text"
              placeholder="輸入文章標題..."
              class="title-input"
              maxlength="200"
          />
          <div class="char-count">{{ article.title.length }}/200</div>
        </div>

        <!-- URL Slug -->
        <div class="form-group">
          <label class="form-label">文章網址 (Slug)</label>
          <div class="slug-input-group">
            <span class="slug-prefix">/blog/</span>
            <input
                v-model="article.slug"
                type="text"
                placeholder="自動產生或手動輸入..."
                class="slug-input"
            />
          </div>
          <div class="form-hint">留空將自動根據標題產生</div>
        </div>

        <!-- 摘要 -->
        <div class="form-group">
          <label class="form-label">文章摘要</label>
          <textarea
              v-model="article.excerpt"
              placeholder="輸入文章摘要，將顯示在列表和分享時..."
              class="excerpt-input"
              rows="3"
              maxlength="300"
          ></textarea>
          <div class="char-count">{{ article.excerpt.length }}/300</div>
        </div>

        <!-- 特色圖片 -->
        <div class="form-group">
          <label class="form-label">特色圖片</label>
          <div class="image-upload">
            <div v-if="article.featuredImageUrl" class="image-preview">
              <img :src="article.featuredImageUrl" alt="特色圖片預覽" />
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
                v-model="article.featuredImageUrl"
                type="url"
                placeholder="或輸入圖片網址..."
                class="image-url-input"
            />
          </div>
        </div>

        <!-- 文字編輯器 -->
        <div class="form-group">
          <label class="form-label">文章內容 *</label>
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
                  <option value="h1">標題 1</option>
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
                  v-html="article.content"
              ></div>
            </div>

            <div v-else class="html-editor">
              <textarea
                  v-model="article.content"
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
          <h3 class="sidebar-title">發布選項</h3>

          <div class="form-group">
            <label class="checkbox-label">
              <input v-model="article.isPublished" type="checkbox" />
              <span class="checkmark"></span>
              立即發布
            </label>
          </div>

          <div class="form-group">
            <label class="checkbox-label">
              <input v-model="article.isFeatured" type="checkbox" />
              <span class="checkmark"></span>
              設為精選文章
            </label>
          </div>
        </div>

        <div class="sidebar-section">
          <h3 class="sidebar-title">統計資訊</h3>
          <div class="stats-info">
            <div class="stat-item">
              <span class="stat-label">字數統計</span>
              <span class="stat-value">{{ wordCount }}</span>
            </div>
            <div class="stat-item" v-if="isEditing">
              <span class="stat-label">瀏覽次數</span>
              <span class="stat-value">{{ article.viewCount || 0 }}</span>
            </div>
          </div>
        </div>

        <div class="sidebar-section" v-if="isEditing">
          <h3 class="sidebar-title">文章操作</h3>
          <div class="action-buttons">
            <button @click="previewArticle" class="preview-btn">
              👁️ 預覽文章
            </button>
            <button @click="deleteArticle" class="delete-btn" :disabled="isDeleting">
              🗑️ {{ isDeleting ? '刪除中...' : '刪除文章' }}
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
import { apiGet, apiPost } from '../utils/api.js'

const route = useRoute()
const router = useRouter()

const isEditing = computed(() => route.params.id !== undefined)
const isSaving = ref(false)
const isDeleting = ref(false)
const showHtml = ref(false)

const article = ref({
  title: '',
  slug: '',
  content: '',
  excerpt: '',
  featuredImageUrl: '',
  isPublished: false,
  isFeatured: false,
  viewCount: 0
})

const editor = ref(null)
const imageInput = ref(null)

onMounted(async () => {
  if (isEditing.value) {
    await loadArticle()
  }
})

const loadArticle = async () => {
  try {
    // const data = await apiGet(`/api/Blog/${route.params.id}`)
    // article.value = data

    // 模擬資料
    article.value = {
      title: '如何申請美國研究所：完整指南',
      slug: 'how-to-apply-us-graduate-school',
      content: '<h2>申請美國研究所的重要步驟</h2><p>申請美國研究所是一個複雜的過程...</p>',
      excerpt: '詳細介紹申請美國研究所的每個步驟，包含文件準備、考試安排等重要資訊',
      featuredImageUrl: 'https://via.placeholder.com/600x400',
      isPublished: true,
      isFeatured: true,
      viewCount: 1250
    }
  } catch (error) {
    console.error('Failed to load article:', error)
  }
}

const wordCount = computed(() => {
  const text = article.value.content.replace(/<[^>]*>/g, '')
  return text.length
})

const updateContent = () => {
  if (editor.value) {
    article.value.content = editor.value.innerHTML
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
      article.value.featuredImageUrl = e.target.result
    }
    reader.readAsDataURL(file)
  }
}

const removeImage = () => {
  article.value.featuredImageUrl = ''
}

const saveDraft = async () => {
  try {
    isSaving.value = true
    article.value.isPublished = false
    await saveArticle()
    alert('草稿已儲存')
  } catch (error) {
    alert('儲存失敗')
  } finally {
    isSaving.value = false
  }
}

const publishArticle = async () => {
  if (!article.value.title.trim()) {
    alert('請輸入文章標題')
    return
  }

  if (!article.value.content.trim()) {
    alert('請輸入文章內容')
    return
  }

  try {
    isSaving.value = true
    article.value.isPublished = true
    await saveArticle()
    alert('文章已發布')
    router.push('/dashboard/blog')
  } catch (error) {
    alert('發布失敗')
  } finally {
    isSaving.value = false
  }
}

const saveArticle = async () => {
  // 自動產生 slug
  if (!article.value.slug && article.value.title) {
    article.value.slug = article.value.title
        .toLowerCase()
        .replace(/[^a-z0-9\s-]/g, '')
        .replace(/\s+/g, '-')
  }

  // 這裡呼叫 API 儲存文章
  console.log('Saving article:', article.value)
  // await apiPost('/api/Blog', article.value)
}

const previewArticle = () => {
  // 開啟預覽視窗
  const previewWindow = window.open('', '_blank')
  const previewContent = `
    <html>
      <head>
        <title>${article.value.title}</title>
        <meta charset="utf-8">
        <style>
          body { font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px; }
          img { max-width: 100%; height: auto; }
        </style>
      </head>
      <body>
        <h1>${article.value.title}</h1>
        ${article.value.featuredImageUrl ? `<img src="${article.value.featuredImageUrl}" alt="特色圖片">` : ''}
        <div>${article.value.content}</div>
      </body>
    </html>
  `
  previewWindow.document.write(previewContent)
}

const deleteArticle = async () => {
  const confirmed = confirm(`確定要刪除文章「${article.value.title}」嗎？此操作無法復原。`)
  if (!confirmed) return

  try {
    isDeleting.value = true
    // await apiDelete(`/api/Blog/${route.params.id}`)
    alert('文章已刪除')
    router.push('/dashboard/blog')
  } catch (error) {
    alert('刪除失敗')
  } finally {
    isDeleting.value = false
  }
}
</script>

<style scoped>
.blog-editor {
  max-width: 1400px;
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
}

.back-link:hover {
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
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  border: 1px solid;
  transition: all 0.2s ease;
}

.save-draft-btn {
  background: white;
  color: #4a5568;
  border-color: #e2e8f0;
}

.save-draft-btn:hover:not(:disabled) {
  background: #f7fafc;
}

.publish-btn {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

.publish-btn:hover:not(:disabled) {
  background: #2c5aa0;
}

.save-draft-btn:disabled, .publish-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.editor-container {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 32px;
}

.editor-main {
  background: white;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  padding: 24px;
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

.title-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 18px;
  font-weight: 600;
}

.title-input:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 3px rgba(49, 130, 206, 0.1);
}

.slug-input-group {
  display: flex;
  align-items: center;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  overflow: hidden;
}

.slug-prefix {
  background: #f7fafc;
  padding: 12px 16px;
  color: #718096;
  font-size: 14px;
  border-right: 1px solid #e2e8f0;
}

.slug-input {
  flex: 1;
  padding: 12px 16px;
  border: none;
  font-size: 14px;
}

.slug-input:focus {
  outline: none;
}

.excerpt-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 14px;
  resize: vertical;
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
  max-width: 300px;
}

.image-preview img {
  width: 100%;
  height: auto;
  border-radius: 6px;
}

.remove-image-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  cursor: pointer;
}

.image-placeholder {
  position: relative;
}

.image-input {
  display: none;
}

.upload-area {
  border: 2px dashed #e2e8f0;
  border-radius: 6px;
  padding: 40px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
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
}

.image-url-input {
  width: 100%;
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 14px;
}

.editor-wrapper {
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  overflow: hidden;
}

.editor-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: #f7fafc;
  border-bottom: 1px solid #e2e8f0;
  flex-wrap: wrap;
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
  padding: 6px 8px;
  border: 1px solid #e2e8f0;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: all 0.2s ease;
}

.toolbar-btn:hover {
  background: #edf2f7;
}

.toolbar-btn.active {
  background: #3182ce;
  color: white;
  border-color: #3182ce;
}

.format-select {
  padding: 6px 8px;
  border: 1px solid #e2e8f0;
  border-radius: 4px;
  font-size: 12px;
}

.rich-editor {
  min-height: 400px;
  padding: 16px;
  outline: none;
  line-height: 1.6;
}

.rich-editor:focus {
  background: #f8fafc;
}

.html-textarea {
  width: 100%;
  min-height: 400px;
  padding: 16px;
  border: none;
  font-family: 'Courier New', monospace;
  font-size: 14px;
  resize: vertical;
}

.html-textarea:focus {
  outline: none;
  background: #f8fafc;
}

.editor-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sidebar-section {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 20px;
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
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
  color: #4a5568;
}

.checkbox-label input[type="checkbox"] {
  margin: 0;
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
  font-size: 14px;
  color: #718096;
}

.stat-value {
  font-size: 14px;
  font-weight: 600;
  color: #2d3748;
}

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.preview-btn, .delete-btn {
  width: 100%;
  padding: 10px 16px;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  border: 1px solid;
  transition: all 0.2s ease;
}

.preview-btn {
  background: #edf2f7;
  color: #4a5568;
  border-color: #e2e8f0;
}

.preview-btn:hover {
  background: #e2e8f0;
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
  .editor-toolbar {
    padding: 8px;
  }

  .toolbar-group {
    padding-right: 4px;
  }

  .rich-editor {
    min-height: 300px;
    padding: 12px;
  }

  .html-textarea {
    min-height: 300px;
    padding: 12px;
  }
}
</style>
import './App.css'
import { Route, Routes } from 'react-router'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'
import { DocumentsPage } from './pages/DocumentsPage'
import { UploadDocumentPage } from './pages/UploadDocumentPage'
import { DocumentDetailsPage } from './pages/DocumentDetailsPage'
import { NotFoundPage } from './pages/NotFoundPage'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="documents">
          <Route index element={<DocumentsPage />} />
          <Route path="upload" element={<UploadDocumentPage />} />
          <Route path=":id" element={<DocumentDetailsPage />} />
        </Route>
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App

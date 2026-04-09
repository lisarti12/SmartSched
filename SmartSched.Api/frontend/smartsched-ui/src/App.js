import { Navigate, Route, Routes } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import StudentHomePage from "./pages/StudentHomePage";
import StudentCoursePage from "./pages/StudentCoursePage";
import StudentCalendarPage from "./pages/StudentCalendarPage";
import StudentAvailabilityPage from "./pages/StudentAvailabilityPage";
import ProfessorHomePage from "./pages/ProfessorHomePage";
import AdminHomePage from "./pages/AdminHomePage";
import ProtectedRoute from "./components/ProtectedRoute";
import StudentSchedulePage from "./pages/StudentSchedulePage";
import ChatPage from "./pages/ChatPage";

export default function App() {
    return (
        <Routes>
            <Route path="/" element={<Navigate to="/login" />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            <Route
                path="/student"
                element={
                    <ProtectedRoute allowedRoles={["Student"]}>
                        <StudentHomePage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/student/courses/:id"
                element={
                    <ProtectedRoute allowedRoles={["Student"]}>
                        <StudentCoursePage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/student/calendar"
                element={
                    <ProtectedRoute allowedRoles={["Student"]}>
                        <StudentCalendarPage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/student/availability"
                element={
                    <ProtectedRoute allowedRoles={["Student"]}>
                        <StudentAvailabilityPage />
                    </ProtectedRoute>
                }
            />
            
            <Route
                path="/chat"
                element={
                    <ProtectedRoute allowedRoles={["Student", "Professor"]}>
                        <ChatPage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/professor"
                element={
                    <ProtectedRoute allowedRoles={["Professor"]}>
                        <ProfessorHomePage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/admin"
                element={
                    <ProtectedRoute allowedRoles={["Admin"]}>
                        <AdminHomePage />
                    </ProtectedRoute>
                }
            />

            <Route
                path="/student/schedule"
                element={
                    <ProtectedRoute allowedRoles={["Student"]}>
                        <StudentSchedulePage />
                    </ProtectedRoute>
                }
            />
        </Routes>
    );
}
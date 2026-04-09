import React, { useEffect, useMemo, useState } from "react";
import axios from "axios";

const API_BASE = "https://localhost:7189/api";

function formatDateKey(date) {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function getMonthMatrix(year, month) {
    const firstDay = new Date(year, month, 1);
    const start = new Date(firstDay);

    const day = start.getDay();
    const mondayOffset = day === 0 ? 6 : day - 1;
    start.setDate(start.getDate() - mondayOffset);
    start.setHours(0, 0, 0, 0);

    const days = [];
    for (let i = 0; i < 42; i++) {
        const current = new Date(start);
        current.setDate(start.getDate() + i);
        days.push(current);
    }

    const weeks = [];
    for (let i = 0; i < 42; i += 7) {
        weeks.push(days.slice(i, i + 7));
    }

    return weeks;
}

function getWeekDays(date) {
    const current = new Date(date);
    const day = current.getDay();
    const mondayOffset = day === 0 ? 6 : day - 1;

    const monday = new Date(current);
    monday.setDate(current.getDate() - mondayOffset);
    monday.setHours(0, 0, 0, 0);

    const days = [];
    for (let i = 0; i < 7; i++) {
        const d = new Date(monday);
        d.setDate(monday.getDate() + i);
        days.push(d);
    }

    return days;
}

function isSameDay(a, b) {
    return (
        a.getFullYear() === b.getFullYear() &&
        a.getMonth() === b.getMonth() &&
        a.getDate() === b.getDate()
    );
}

function getDisplayTime(dateValue) {
    if (!dateValue) return "";
    const d = new Date(dateValue);

    if (
        d.getHours() === 0 &&
        d.getMinutes() === 0 &&
        d.getSeconds() === 0
    ) {
        return "";
    }

    return d.toLocaleTimeString("en-US", {
        hour: "2-digit",
        minute: "2-digit",
    });
}

function StudentCalendarPage() {
    const [currentDate, setCurrentDate] = useState(new Date());
    const [items, setItems] = useState([]);
    const [courses, setCourses] = useState([]);
    const [loading, setLoading] = useState(false);
    const [view, setView] = useState("month");

    const token = localStorage.getItem("token");

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    const monthMatrix = useMemo(function () {
        return getMonthMatrix(year, month);
    }, [year, month]);

    const weekDays = useMemo(function () {
        return getWeekDays(currentDate);
    }, [currentDate]);

    useEffect(function () {
        loadPageData();
    }, [currentDate, view]);

    async function loadPageData() {
        try {
            setLoading(true);

            const selectedDate = formatDateKey(currentDate);

            const requests = [
                axios.get(
                    `${API_BASE}/student/calendar?view=${encodeURIComponent(view)}&date=${encodeURIComponent(selectedDate)}`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`,
                        },
                    }
                ),
                axios.get(`${API_BASE}/student/courses`, {
                    headers: {
                        Authorization: `Bearer ${token}`,
                    },
                }),
            ];

            const [calendarRes, coursesRes] = await Promise.all(requests);

            const calendarItems = Array.isArray(calendarRes.data?.items)
                ? calendarRes.data.items
                : Array.isArray(calendarRes.data)
                    ? calendarRes.data
                    : [];

            const studentCourses = Array.isArray(coursesRes.data)
                ? coursesRes.data
                : Array.isArray(coursesRes.data?.items)
                    ? coursesRes.data.items
                    : [];

            setCourses(studentCourses);
            setItems(normalizeCalendarItems(calendarItems, studentCourses));
        } catch (err) {
            console.error("Failed to load calendar:", err);
            setCourses([]);
            setItems([]);
        } finally {
            setLoading(false);
        }
    }

    function normalizeCalendarItems(rawItems, studentCourses) {
        const normalized = rawItems
            .map(function (item, index) {
                const rawDate =
                    item.date ||
                    item.dueDate ||
                    item.deadline ||
                    item.deadlineDate ||
                    item.scheduledDate ||
                    item.scheduledFor ||
                    item.startDate ||
                    item.eventDate;

                if (!rawDate) {
                    return null;
                }

                const matchedCourse =
                    studentCourses.find(function (course) {
                        return (
                            String(course.id) === String(item.courseId) ||
                            String(course.id) === String(item.classId) ||
                            String(course.courseId) === String(item.courseId) ||
                            String(course.classId) === String(item.classId) ||
                            (course.title &&
                                item.courseName &&
                                course.title.trim().toLowerCase() === item.courseName.trim().toLowerCase()) ||
                            (course.name &&
                                item.courseName &&
                                course.name.trim().toLowerCase() === item.courseName.trim().toLowerCase())
                        );
                    }) || null;

                const courseId =
                    item.courseId ||
                    item.classId ||
                    item.course?.id ||
                    item.class?.id ||
                    matchedCourse?.id ||
                    matchedCourse?.courseId ||
                    matchedCourse?.classId ||
                    null;

                const courseName =
                    item.courseName ||
                    item.className ||
                    item.courseTitle ||
                    item.course?.title ||
                    item.class?.title ||
                    matchedCourse?.title ||
                    matchedCourse?.name ||
                    "";

                const type =
                    item.type ||
                    item.itemType ||
                    item.category ||
                    (item.isDeadline ? "Deadline" : null) ||
                    "Item";

                const title =
                    item.title ||
                    item.assignmentTitle ||
                    item.taskTitle ||
                    item.name ||
                    "Untitled";

                return {
                    id: item.id || `${title}-${rawDate}-${index}`,
                    raw: item,
                    title: title,
                    type: type,
                    date: rawDate,
                    dateKey: formatDateKey(rawDate),
                    timeText: getDisplayTime(rawDate),
                    courseId: courseId,
                    courseName: courseName,
                    isClickable:
                        type === "Deadline" ||
                        type === "Assignment" ||
                        type === "Homework" ||
                        type === "Quiz" ||
                        type === "Project" ||
                        Boolean(courseId),
                };
            })
            .filter(Boolean);

        const uniqueMap = new Map();

        normalized.forEach(function (item) {
            const uniqueKey = [
                item.title,
                item.type,
                item.dateKey,
                item.timeText,
                item.courseId || "",
                item.courseName || ""
            ].join("|");

            if (!uniqueMap.has(uniqueKey)) {
                uniqueMap.set(uniqueKey, item);
            }
        });

        return Array.from(uniqueMap.values());
    }

    const itemsByDate = useMemo(function () {
        const map = {};

        for (const item of items) {
            if (!map[item.dateKey]) {
                map[item.dateKey] = [];
            }

            map[item.dateKey].push(item);
        }

        Object.keys(map).forEach(function (key) {
            map[key].sort(function (a, b) {
                return new Date(a.date) - new Date(b.date);
            });
        });

        return map;
    }, [items]);

    function goBack() {
        window.history.back();
    }

    function goPrev() {
        if (view === "month") {
            setCurrentDate(new Date(year, month - 1, 1));
        } else if (view === "week") {
            const prev = new Date(currentDate);
            prev.setDate(prev.getDate() - 7);
            setCurrentDate(prev);
        } else {
            const prev = new Date(currentDate);
            prev.setDate(prev.getDate() - 1);
            setCurrentDate(prev);
        }
    }

    function goNext() {
        if (view === "month") {
            setCurrentDate(new Date(year, month + 1, 1));
        } else if (view === "week") {
            const next = new Date(currentDate);
            next.setDate(next.getDate() + 7);
            setCurrentDate(next);
        } else {
            const next = new Date(currentDate);
            next.setDate(next.getDate() + 1);
            setCurrentDate(next);
        }
    }

    function goToday() {
        setCurrentDate(new Date());
    }

    function getTitle() {
        if (view === "month") {
            return currentDate.toLocaleString("en-US", {
                month: "long",
                year: "numeric",
            });
        }

        if (view === "week") {
            const start = weekDays[0];
            const end = weekDays[6];

            return `${start.toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
            })} - ${end.toLocaleDateString("en-US", {
                month: "short",
                day: "numeric",
                year: "numeric",
            })}`;
        }

        return currentDate.toLocaleDateString("en-US", {
            weekday: "long",
            month: "long",
            day: "numeric",
            year: "numeric",
        });
    }

    function getItemColor(type) {
        if (type === "Deadline") return "#ffe5e5";
        if (type === "Assignment") return "#fff3cd";
        if (type === "Homework") return "#fff3cd";
        if (type === "Quiz") return "#f3e8ff";
        if (type === "Project") return "#e0f2fe";
        if (type === "ScheduledTask") return "#e5f0ff";
        if (type === "Holiday") return "#e9f9e5";
        return "#f3f3f3";
    }

    function getCourseUrl(item) {
        if (!item.courseId) return null;

        // CHANGE ONLY THIS LINE if your real course page route is different
        return `/student/courses/${item.courseId}`;
    }

    function openItem(item) {
        const url = getCourseUrl(item);
        if (!url) return;
        window.location.href = url;
    }

    function renderItem(item, idx) {
        const clickable = item.isClickable && item.courseId;
        const courseUrl = getCourseUrl(item);

        return (
            <div
                key={item.id || idx}
                onClick={clickable ? function () { openItem(item); } : undefined}
                title={
                    clickable
                        ? `${item.title} - ${item.courseName} (Click to open class)`
                        : `${item.title} - ${item.courseName}`
                }
                style={{
                    fontSize: "12px",
                    padding: "6px 8px",
                    borderRadius: "8px",
                    marginBottom: "6px",
                    background: getItemColor(item.type),
                    overflow: "hidden",
                    textOverflow: "ellipsis",
                    whiteSpace: "nowrap",
                    cursor: clickable ? "pointer" : "default",
                    border: clickable ? "1px solid #d6d6d6" : "1px solid transparent",
                    transition: "0.15s ease",
                }}
            >
                <div style={{ fontWeight: "700" }}>{item.title}</div>
                <div style={{ opacity: 0.8 }}>{item.courseName}</div>
                <div style={{ opacity: 0.7 }}>
                    {item.type}
                    {item.timeText ? ` • ${item.timeText}` : ""}
                </div>
                {clickable && courseUrl ? (
                    <div
                        style={{
                            marginTop: "4px",
                            fontSize: "11px",
                            color: "#2563eb",
                            fontWeight: "600",
                        }}
                    >
                        Open class
                    </div>
                ) : null}
            </div>
        );
    }

    function renderMonthView() {
        return (
            <>
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "repeat(7, 1fr)",
                        marginBottom: "8px",
                        fontWeight: "bold",
                        textAlign: "center",
                    }}
                >
                    <div>Mon</div>
                    <div>Tue</div>
                    <div>Wed</div>
                    <div>Thu</div>
                    <div>Fri</div>
                    <div>Sat</div>
                    <div>Sun</div>
                </div>

                <div style={{ display: "grid", gap: "8px" }}>
                    {monthMatrix.map(function (week, weekIndex) {
                        return (
                            <div
                                key={weekIndex}
                                style={{
                                    display: "grid",
                                    gridTemplateColumns: "repeat(7, 1fr)",
                                    gap: "8px",
                                }}
                            >
                                {week.map(function (day) {
                                    const key = formatDateKey(day);
                                    const dayItems = itemsByDate[key] || [];
                                    const isCurrentMonth = day.getMonth() === month;
                                    const isTodayCell = isSameDay(day, new Date());

                                    return (
                                        <div
                                            key={key}
                                            style={{
                                                minHeight: "140px",
                                                border: isTodayCell ? "2px solid #2563eb" : "1px solid #ddd",
                                                borderRadius: "10px",
                                                padding: "8px",
                                                background: isCurrentMonth ? "#ffffff" : "#f5f5f5",
                                            }}
                                        >
                                            <div
                                                style={{
                                                    fontWeight: "bold",
                                                    marginBottom: "8px",
                                                    color: isCurrentMonth ? "#000" : "#888",
                                                }}
                                            >
                                                {day.getDate()}
                                            </div>

                                            {dayItems.length === 0 ? null : dayItems.map(renderItem)}
                                        </div>
                                    );
                                })}
                            </div>
                        );
                    })}
                </div>
            </>
        );
    }

    function renderWeekView() {
        return (
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(7, 1fr)",
                    gap: "8px",
                }}
            >
                {weekDays.map(function (day) {
                    const key = formatDateKey(day);
                    const dayItems = itemsByDate[key] || [];
                    const isTodayCell = isSameDay(day, new Date());

                    return (
                        <div
                            key={key}
                            style={{
                                minHeight: "360px",
                                border: isTodayCell ? "2px solid #2563eb" : "1px solid #ddd",
                                borderRadius: "10px",
                                padding: "8px",
                                background: "#ffffff",
                            }}
                        >
                            <div
                                style={{
                                    fontWeight: "bold",
                                    marginBottom: "10px",
                                    paddingBottom: "8px",
                                    borderBottom: "1px solid #eee",
                                }}
                            >
                                <div>
                                    {day.toLocaleDateString("en-US", {
                                        weekday: "short",
                                    })}
                                </div>
                                <div>
                                    {day.toLocaleDateString("en-US", {
                                        month: "short",
                                        day: "numeric",
                                    })}
                                </div>
                            </div>

                            {dayItems.length === 0 ? (
                                <div style={{ fontSize: "12px", color: "#888" }}>No items</div>
                            ) : (
                                dayItems.map(renderItem)
                            )}
                        </div>
                    );
                })}
            </div>
        );
    }

    function renderDayView() {
        const key = formatDateKey(currentDate);
        const dayItems = itemsByDate[key] || [];

        return (
            <div
                style={{
                    background: "#ffffff",
                    border: "1px solid #ddd",
                    borderRadius: "12px",
                    padding: "16px",
                    minHeight: "420px",
                }}
            >
                <div
                    style={{
                        fontWeight: "bold",
                        fontSize: "18px",
                        marginBottom: "16px",
                    }}
                >
                    {currentDate.toLocaleDateString("en-US", {
                        weekday: "long",
                        month: "long",
                        day: "numeric",
                        year: "numeric",
                    })}
                </div>

                {dayItems.length === 0 ? (
                    <div style={{ color: "#888" }}>No items for this day</div>
                ) : (
                    dayItems.map(renderItem)
                )}
            </div>
        );
    }

    return (
        <div style={{ padding: "24px" }}>
            <div
                style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: "20px",
                    flexWrap: "wrap",
                    gap: "12px",
                }}
            >
                <div
                    style={{
                        display: "flex",
                        gap: "12px",
                        alignItems: "center",
                        flexWrap: "wrap",
                    }}
                >
                    <button onClick={goBack}>Back</button>
                    <button onClick={goPrev}>Previous</button>
                    <button onClick={goToday}>Today</button>
                    <button onClick={goNext}>Next</button>
                    <h3 style={{ margin: 0 }}>{getTitle()}</h3>
                </div>

                <div
                    style={{
                        display: "flex",
                        gap: "8px",
                        alignItems: "center",
                    }}
                >
                    <button
                        onClick={function () { setView("day"); }}
                        style={{
                            background: view === "day" ? "#2563eb" : "#f0f0f0",
                            color: view === "day" ? "#fff" : "#000",
                            border: "none",
                            padding: "8px 14px",
                            borderRadius: "8px",
                            cursor: "pointer",
                        }}
                    >
                        Day
                    </button>

                    <button
                        onClick={function () { setView("week"); }}
                        style={{
                            background: view === "week" ? "#2563eb" : "#f0f0f0",
                            color: view === "week" ? "#fff" : "#000",
                            border: "none",
                            padding: "8px 14px",
                            borderRadius: "8px",
                            cursor: "pointer",
                        }}
                    >
                        Week
                    </button>

                    <button
                        onClick={function () { setView("month"); }}
                        style={{
                            background: view === "month" ? "#2563eb" : "#f0f0f0",
                            color: view === "month" ? "#fff" : "#000",
                            border: "none",
                            padding: "8px 14px",
                            borderRadius: "8px",
                            cursor: "pointer",
                        }}
                    >
                        Month
                    </button>
                </div>
            </div>

            <h2>My Calendar</h2>

            {loading && <p>Loading...</p>}

            {!loading && view === "month" && renderMonthView()}
            {!loading && view === "week" && renderWeekView()}
            {!loading && view === "day" && renderDayView()}
        </div>
    );
}

export default StudentCalendarPage;
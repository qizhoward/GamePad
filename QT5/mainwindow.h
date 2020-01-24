#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "gamepad_thread.h"

namespace Ui
{
class MainWindow;
}

class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget *parent = nullptr);
    ~MainWindow();

private slots:
    void display_slot_row(joyinfoex_tag state_row);

    void on_MainWindow_tabifiedDockWidgetActivated(QDockWidget *dockWidget);

    void open();
    void saveAs();
    void about();

private:
    void clearDisplay();
    Ui::MainWindow *ui;

    void createMenus();







};

#endif // MAINWINDOW_H


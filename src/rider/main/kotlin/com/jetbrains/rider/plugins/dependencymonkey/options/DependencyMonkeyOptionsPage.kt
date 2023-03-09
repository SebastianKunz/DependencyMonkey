package com.jetbrains.rider.plugins.dependencymonkey.options

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class DependencyMonkeyOptionsPage : SimpleOptionsPage(
    name = "DependencyMonkey",
    pageId = "DependencyMonkeyOptionsPage" // Must be in sync with backend
) {
    override fun getId(): String {
        return "DependencyMonkeyOptionsPage"
    }
}
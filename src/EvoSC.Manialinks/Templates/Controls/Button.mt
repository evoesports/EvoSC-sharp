﻿<!--
    Simple clickable button.
-->
<component>
    <!-- The ID of the button. -->
    <property type="string" name="id" />
    
    <!-- Text to display in the button -->
    <property type="string" name="text" />
    
    <!-- X position of the button. -->
    <property type="double" name="x" default="0.0" />
    
    <!-- Y position of the button. -->
    <property type="double" name="y" default="0.0" />
    
    <!-- Width of the button background. -->
    <property type="double" name="width" default="17.0" />
    
    <!-- Height of the button background. -->
    <property type="double" name="height" default="5.0" />
    
    <!-- The button style type, can be default or secondary. -->
    <property type="string" name="type" default="default" />
    
    <property type="string?" name="bgColor" default="null" />
    
    <!-- The action to call when clicking the button. This disables script events. -->
    <property type="string" name="action" default="" />
    
    <!-- Whether the button is disabled or not. If disabled, the button wont fire events. -->
    <property type="bool" name="disabled" default="false" />
    
    <!-- Custom style -->
    <property type="string" name="className" default="evosc-button" />
    
    <template>
        <frame id="{{ id }}-frame" pos="{{ x }} {{ y }}" class="{{ className }}-frame" data-disabled="{{ disabled }}">
            <frame if="!disabled">
                <quad
                        class='{{ className }}-bg-default {{ type == "secondary" ? "btn-secondary" : "btn-default" }}'
                        size="{{ width }} {{ height }}"
                        scriptevents="1"
                        bgcolor='{{ bgColor == null ? "" : bgColor }}'
                />
                <label
                        size="{{ width }} {{ height }}"
                        class='{{ className }}-btn-default {{ type == "secondary" ? "btn-secondary" : "btn-default" }}'
                        text="{{ text }}"
                        scriptevents="1"
                        halign="center"
                        valign="center"
                        pos="{{ width/2 }} {{ -height/2 }}"
                        if='action.Equals("", StringComparison.Ordinal)'
                        data-id="{{ id }}"
                        id="{{ id }}"
                />
                <label
                        class='{{ className }}-btn-default {{ type == "secondary" ? "btn-secondary" : "btn-default" }}'
                        size="{{ width }} {{ height }}"
                        text="{{ text }}"
                        scriptevents="1"
                        action="{{ action }}"
                        halign="center"
                        valign="center"
                        pos="{{ width/2 }} {{ -height/2 }}"
                        if='!action.Equals("", StringComparison.Ordinal)'
                        data-id="{{ id }}"
                        id="{{ id }}"
                />
            </frame>
            <frame if="disabled">
                <quad
                        size="{{ width }} {{ height }}"
                        class='{{ className }}-bg-disabled {{ type == "secondary" ? "btn-secondary-disabled" : "btn-disabled" }}'
                />
                <label
                        id="{{ id }}"
                        class='{{ className }}-btn-disabled {{ type == "secondary" ? "btn-secondary-disabled" : "btn-disabled" }}'
                        size="{{ width }} {{ height }}"
                        text="{{ text }}"
                        halign="center"
                        valign="center"
                        pos="{{ width/2 }} {{ -height/2 }}"
                />
            </frame>
        </frame>
    </template>
</component>

﻿<component>
    <import component="EvoSC.Theme" as="Theme" />
    <import component="EvoSC.HiddenEntry" as="HiddenEntry" />

    <property type="string" name="id" />
    <property type="double" name="x" default="0.0"/>
    <property type="double" name="y" default="0.0"/>
    <property type="bool" name="value" default="false"/>
    
    <template>
        <Theme />
        <frame pos="{{ x }} {{ y }}" size="10 5" scriptevents="1" id="{{ id }}" class="evosc-toggleswitch-frame">
            <quad 
                    class='{{ value ? "toggleswitch-on-default" : "toggleswitch-off-default" }} evosc-toggleswitch'
                    size="10 5"
                    scriptevents="1"
                    data-id="{{ id }}"
            />
            <quad 
                    bgcolor="FAE1EA"
                    pos="{{ value ? 5 : 0 }} 0"
                    size="5 5"
                    scriptevents="1"
                    class="evosc-toggleswitch"
                    data-id="{{ id }}"
            />
            <label
                    class='{{ value ? "toggleswitch-on-default" : "toggleswitch-off-default" }} evosc-toggleswitch'
                    text='{{ value ? "" : "" }}'
                    valign="center"
                    halign="center"
                    pos="{{ value ? 7.5 : 2.5 }} -2.1"
                    textsize="1.5"
                    scriptevents="1"
                    data-id="{{ id }}"
            />
            <HiddenEntry 
                    name="{{ id }}"
                    value="{{ value }}"
            />
        </frame>
    </template>

    <script resource="EvoSC.Scripts.Switch" once="true"/>
</component>

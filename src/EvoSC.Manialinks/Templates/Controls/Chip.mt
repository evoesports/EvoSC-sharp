<component>
  <import component="EvoSC.Controls.Tag" as="Tag" />
  
  <property type="string" name="id" default="evosc_chip" />
  <property type="string" name="text" default="" />
  <property type="double" name="x" default="0.0" />
  <property type="double" name="y" default="0.0" />
  <property type="double" name="width" default="15" />
  <property type="string" name="style" default="Square" /> <!-- Styles: Round, Square -->
  <property type="string" name="closable" default="false" />
  <property type="bool" name="hidden" default="false" />
  <property type="string" name="severity" default="primary" /> <!-- primary, secondary, success, info, warning, danger -->
  <property type="string?" name="bgColor" default="null" />
  <property type="string?" name="textColor" default="null" />
  
  <template>
    <Tag 
            custom="true" 
            id="{{ id }}"
            text="{{ text }}"
            x="{{ x }}"
            y="{{ y }}"
            width="{{ width }}"
            style="{{ style }}"
            closable="{{ closable }}"
            hidden="{{ hidden }}"
            severity="{{ severity }}"
            bgColor="{{ bgColor }}"
            textColor="{{ textColor }}"
    >
      <template slot="content">
        <label
                text="{{ Icons.TimesCircle }}"
                textcolor="{{ textColor == null ? Util.TypeToColorText(severity) : textColor }}"
                textsize="0.7"
                pos="{{ width-3 }} -1.35"
                valign="center"
                scriptevents="1"
                class="chip-btnClose"
        />
      </template>
    </Tag>
    <!-- <Panel
            x="{{ x }}"
            y="{{ y }}"
            id="{{ id }}"
            width="{{ width }}"
            height="3"
            cornerRadius='{{ style == "Round" ? 2.5 : 0 }}'
            bgColor="{{ Theme.UI_Chip_Default_Bg }}"
            data="{{ closable }}"
            hidden="{{ hidden }}"
    >
      <label 
              class="text"
              pos="0.4 -1.2" 
              textcolor="{{ Theme.UI_Chip_Default_Text }}"
              size="{{ width }} 5"
              text="{{ text }}" 
              textsize="1"
              valign="center"
      />
      
      <label 
              text="{{ Icons.TimesCircle }}"
              textcolor="{{ Theme.UI_Chip_Default_Text }}"
              textsize="0.7"
              pos="{{ width-3 }} -1.35"
              valign="center"
              scriptevents="1"
              class="chip-btnClose"
      />
    </Panel> -->
  </template>
  
  <script resource="EvoSC.Scripts.Chip" once="true" />
</component>